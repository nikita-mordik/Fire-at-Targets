using System;
using System.Collections.Generic;
using Demo.Scripts.Runtime.AttachmentSystem;
using Demo.Scripts.Runtime.Character;
using Demo.Scripts.Runtime.Item;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.StaticData;
using FreedLOW.FireAtTargets.Code.Weapon.Shooting;
using KINEMATION.FPSAnimationFramework.Runtime.Camera;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using KINEMATION.KAnimationCore.Runtime.Input;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon
{
    public class CustomWeapon : FPSItem
    {
        [Header("Weapon data")]
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private RayShooting rayShooting;
        
        [Header("General")]
        [SerializeField] [Range(0f, 120f)] private float fieldOfView = 90f;
        [SerializeField] private FPSAnimationAsset reloadClip;
        [SerializeField] private FPSCameraAnimation cameraReloadAnimation;
        [SerializeField] private FPSAnimationAsset grenadeClip;
        [SerializeField] private FPSCameraAnimation cameraGrenadeAnimation;

        [Header("Recoil")]
        [SerializeField] private FPSAnimationAsset fireClip;
        [SerializeField] private RecoilAnimData recoilData;
        [SerializeField] private RecoilPatternSettings recoilPatternSettings;
        [SerializeField] private FPSCameraShake cameraShake;
        [SerializeField] private bool supportsAuto;

        [Header("Attachments")] 
        [SerializeField] private AttachmentGroup<BaseAttachment> barrelAttachments = new();
        [SerializeField] private AttachmentGroup<BaseAttachment> gripAttachments = new();
        [SerializeField] private List<AttachmentGroup<ScopeAttachment>> scopeGroups = new();

        private static readonly int CurveEquip = Animator.StringToHash("CurveEquip");
        private static readonly int CurveUnequip = Animator.StringToHash("CurveUnequip");
        
        private FPSController _fpsController;
        private Animator _controllerAnimator;
        private UserInputController _userInputController;
        private IPlayablesController _playablesController;
        private FPSCameraController _fpsCameraController;

        private FPSAnimator _fpsAnimator;
        private FPSAnimatorEntity _fpsAnimatorEntity;

        private RecoilAnimation _recoilAnimation;
        private RecoilPattern _recoilPattern;

        private Animator _weaponAnimator;
        private int _scopeIndex;

        private float _lastRecoilTime;
        private bool _supportsBurst;
        private int _bursts;
        private int _burstLength;
        private FireMode _fireMode;

        private float _fireRate;
        private int _startAmmo;
        private int _currentAmmo;
        private int _currentMaxAmmo;

        private bool _isCurrentlyReload;

        public int MaxAmmo { get; private set; }
        public int CurrentAmmo
        {
            get => _currentAmmo;
            private set
            {
                if (value < 0)
                    throw new Exception("Incorrect ammo value!");

                _currentAmmo = value;
                _weaponEventHandlerService.InvokeOnCurrentAmmoChanged(_currentAmmo);
            }
        }

        private IWeaponEventHandlerService _weaponEventHandlerService;

        [Inject]
        private void Construct(IWeaponEventHandlerService weaponEventHandlerService)
        {
            _weaponEventHandlerService = weaponEventHandlerService;
        }

        private void Awake()
        {
            _fireRate = weaponData.FireRate;
            _fireMode = weaponData.FireMode;
            _supportsBurst = weaponData.SupportsBurst;
            _bursts = weaponData.BurstAmount;
            _burstLength = _bursts;
            rayShooting.DamageAmount = weaponData.Damage;
            MaxAmmo = weaponData.MaxAmmo;
            _currentMaxAmmo = MaxAmmo;
            _startAmmo = weaponData.StartAmmo;
            CurrentAmmo = _startAmmo;
        }

        public override void OnEquip(GameObject parent)
        {
            if (parent == null) 
                return;
            
            _fpsAnimator = parent.GetComponent<FPSAnimator>();
            _fpsAnimatorEntity = GetComponent<FPSAnimatorEntity>();
            
            _fpsController = parent.GetComponent<FPSController>();
            _weaponAnimator = GetComponentInChildren<Animator>();
            
            _controllerAnimator = parent.GetComponent<Animator>();
            _userInputController = parent.GetComponent<UserInputController>();
            _playablesController = parent.GetComponent<IPlayablesController>();
            _fpsCameraController = parent.GetComponentInChildren<FPSCameraController>();

            if (overrideController != _controllerAnimator.runtimeAnimatorController)
            {
                _playablesController.UpdateAnimatorController(overrideController);
            }
            
            InitializeAttachments();
            
            _recoilAnimation = parent.GetComponent<RecoilAnimation>();
            _recoilPattern = parent.GetComponent<RecoilPattern>();
            
            _fpsAnimator.LinkAnimatorProfile(gameObject);
            
            barrelAttachments.Initialize(_fpsAnimator);
            gripAttachments.Initialize(_fpsAnimator);
            
            _recoilAnimation.Init(recoilData, _fireRate, _fireMode);

            if (_recoilPattern != null)
            {
                _recoilPattern.Init(recoilPatternSettings);
            }
            
            _fpsAnimator.LinkAnimatorLayer(equipMotion);
            
            InvokeWhenEquip();
        }

        public override void OnUnEquip()
        {
            _fpsAnimator.LinkAnimatorLayer(unEquipMotion);
        }

        public override bool OnAimPressed()
        {
            _userInputController.SetValue(FPSANames.IsAiming, true);
            UpdateTargetFOV(true);
            _recoilAnimation.isAiming = true;
            
            return true;
        }

        public override bool OnAimReleased()
        {
            _userInputController.SetValue(FPSANames.IsAiming, false);
            UpdateTargetFOV(false);
            _recoilAnimation.isAiming = false;
            
            return true;
        }

        public override bool OnFirePressed()
        {
            if (_isCurrentlyReload || CurrentAmmo <= 0)
                return false;
            
            // Do not allow firing faster than the allowed fire rate.
            if (Time.unscaledTime - _lastRecoilTime < 60f / _fireRate)
                return false;
            
            _lastRecoilTime = Time.unscaledTime;
            _bursts = _burstLength;
            
            OnFire();
            
            return true;
        }

        public override bool OnFireReleased()
        {
            if (_recoilAnimation != null)
            {
                _recoilAnimation.Stop();
            }
            
            if (_recoilPattern != null)
            {
                _recoilPattern.OnFireEnd();
            }
            
            CancelInvoke(nameof(OnFire));
            return true;
        }

        public override bool OnReload()
        {
            if (_currentMaxAmmo <= 0)
                return false;
            
            if (!FPSAnimationAsset.IsValid(reloadClip))
                return false;

            _isCurrentlyReload = true;
            
            _playablesController.PlayAnimation(reloadClip, 0f);
            
            if (_weaponAnimator != null)
            {
                _weaponAnimator.Rebind();
                _weaponAnimator.Play("Reload", 0);
            }

            if (_fpsCameraController != null)
            {
                _fpsCameraController.PlayCameraAnimation(cameraReloadAnimation);
            }
            
            CurrentAmmo = _startAmmo;
            _currentMaxAmmo -= CurrentAmmo;
            _weaponEventHandlerService.InvokeOnReload(_currentMaxAmmo);
            
            Invoke(nameof(OnActionEnded), reloadClip.clip.length * 0.85f);

            OnFireReleased();
            return true;
        }

        public override bool OnGrenadeThrow()
        {
            if (!FPSAnimationAsset.IsValid(grenadeClip))
            {
                return false;
            }

            _playablesController.PlayAnimation(grenadeClip, 0f);
            
            if (_fpsCameraController != null)
            {
                _fpsCameraController.PlayCameraAnimation(cameraGrenadeAnimation);
            }
            
            Invoke(nameof(OnActionEnded), grenadeClip.clip.length * 0.8f);
            return true;
        }

        public override void OnCycleScope()
        {
            if (scopeGroups.Count == 0) 
                return;
            
            _scopeIndex++;
            _scopeIndex = _scopeIndex > scopeGroups.Count - 1 ? 0 : _scopeIndex;
            
            UpdateAimPoint();
            UpdateTargetFOV(true);
        }

        public override void OnChangeFireMode()
        {
            CycleFireMode();
            _recoilAnimation.fireMode = _fireMode;
        }

        public override void OnAttachmentChanged(int attachmentTypeIndex)
        {
            if (attachmentTypeIndex == 1)
            {
                barrelAttachments.CycleAttachments(_fpsAnimator);
                return;
            }

            if (attachmentTypeIndex == 2)
            {
                gripAttachments.CycleAttachments(_fpsAnimator);
                return;
            }

            if (scopeGroups.Count == 0) 
                return;
            
            scopeGroups[_scopeIndex].CycleAttachments(_fpsAnimator);
            UpdateAimPoint();
        }
        
        public bool HasMagazineAmmo() => _currentMaxAmmo > 0;

        private void UpdateTargetFOV(bool isAiming)
        {
            float fov = fieldOfView;
            float sensitivityMultiplier = 1f;
            
            if (isAiming && scopeGroups.Count != 0)
            {
                var scope = scopeGroups[_scopeIndex].GetActiveAttachment();
                fov *= scope.aimFovZoom;
                sensitivityMultiplier = scopeGroups[_scopeIndex].GetActiveAttachment().sensitivityMultiplier;
            }

            _userInputController.SetValue("SensitivityMultiplier", sensitivityMultiplier);
            _fpsCameraController.UpdateTargetFOV(fov);
        }

        private void UpdateAimPoint()
        {
            if (scopeGroups.Count == 0) 
                return;

            var scope = scopeGroups[_scopeIndex].GetActiveAttachment().aimPoint;
            _fpsAnimatorEntity.defaultAimPoint = scope;
        }

        private void InitializeAttachments()
        {
            foreach (var attachmentGroup in scopeGroups)
            {
                attachmentGroup.Initialize(_fpsAnimator);
            }
            
            _scopeIndex = 0;
            if (scopeGroups.Count == 0) 
                return;

            UpdateAimPoint();
            UpdateTargetFOV(false);
        }

        private void OnActionEnded()
        {
            if (_fpsController == null) 
                return;

            _isCurrentlyReload = false;
            _fpsController.ResetActionState();
        }

        private void OnFire()
        {
            if (_weaponAnimator != null) 
                _weaponAnimator.Play("Fire", 0, 0f);
            
            _fpsCameraController.PlayCameraShake(cameraShake);
            
            if(fireClip != null) _playablesController.PlayAnimation(fireClip);

            if (_recoilAnimation != null && recoilData != null)
            {
                _recoilAnimation.Play();
            }

            if (_recoilPattern != null)
            {
                _recoilPattern.OnFireStart();
            }
            
            rayShooting.OnFire();

            if (_recoilAnimation.fireMode == FireMode.Semi)
            {
                Invoke(nameof(OnFireReleased), 60f / _fireRate);
                return;
            }
            
            if (_recoilAnimation.fireMode == FireMode.Burst)
            {
                _bursts--;
                CurrentAmmo--;
                
                if (_bursts == 0)
                {
                    OnFireReleased();
                    
                    if (CurrentAmmo <= 0)
                    {
                        OnFireReleased();
                        OnReload();
                    }
                    
                    return;
                }
            }
            else
            {
                CurrentAmmo--;
            }
            
            if (CurrentAmmo <= 0)
            {
                OnFireReleased();
                OnReload();
                return;
            }
            
            Invoke(nameof(OnFire), 60f / _fireRate);
        }

        private void CycleFireMode()
        {
            if (_fireMode == FireMode.Semi && _supportsBurst)
            {
                _fireMode = FireMode.Burst;
                _bursts = _burstLength;
                return;
            }

            if (_fireMode != FireMode.Auto && supportsAuto)
            {
                _fireMode = FireMode.Auto;
                return;
            }

            _fireMode = FireMode.Semi;
        }

        private void InvokeWhenEquip()
        {
            _weaponEventHandlerService.InvokeOnCurrentAmmoChanged(_currentAmmo);
            _weaponEventHandlerService.InvokeOnReload(_currentMaxAmmo);
        }
    }
}