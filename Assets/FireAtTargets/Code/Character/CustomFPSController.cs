using Demo.Scripts.Runtime.Character;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using KINEMATION.KAnimationCore.Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreedLOW.FireAtTargets.Code.Character
{
    [RequireComponent(typeof(CharacterController), typeof(CustomFPSMovement))]
    public class CustomFPSController : MonoBehaviour
    {
        [SerializeField] private FPSControllerSettings _settings;
        [SerializeField] private CharacterWeaponController _weaponController;
        [SerializeField] private bool _isLockCursor;
        
        private static readonly int FullBodyWeightHash = Animator.StringToHash("FullBodyWeight");
        private static readonly int ProneWeightHash = Animator.StringToHash("ProneWeight");
        private static readonly int InspectStartHash = Animator.StringToHash("InspectStart");
        private static readonly int InspectEndHash = Animator.StringToHash("InspectEnd");
        private static readonly int SlideHash = Animator.StringToHash("Sliding");

        private CustomFPSMovement _movementComponent;

        private Vector2 _playerInput;

        private FPSAimState _aimState;
        private FPSActionState _actionState;

        private Animator _animator;

        private FPSAnimator _fpsAnimator;
        private UserInputController _userInput;

        private Vector2 _lookDeltaInput;

        private RecoilPattern _recoilPattern;
        private int _sensitivityMultiplierPropertyIndex;
        
        private bool _isLeaning;

        private void Start()
        {
            if (_isLockCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            Time.timeScale = _settings.timeScale;
            Application.targetFrameRate = 144;
            
            GetComponents();
            InitializeMovement();

            _actionState = FPSActionState.None;
            _sensitivityMultiplierPropertyIndex = _userInput.GetPropertyIndex("SensitivityMultiplier");
        }

        private void Update()
        {
            UpdateLookInput();
            OnMovementUpdated();
        }
        
        public void ResetActionState()
        {
            _actionState = FPSActionState.None;
        }

        private void GetComponents()
        {
            _fpsAnimator = GetComponent<FPSAnimator>();
            _animator = GetComponent<Animator>();
            _userInput = GetComponent<UserInputController>();
            _recoilPattern = GetComponent<RecoilPattern>();
        }

        private void PlayTransitionMotion(FPSAnimatorLayerSettings layerSettings)
        {
            if (!layerSettings)
                return;
            
            _fpsAnimator.LinkAnimatorLayer(layerSettings);
        }

        private bool IsSprinting()
        {
            return _movementComponent.MovementState == FPSMovementState.Sprinting;
        }

        private bool HasActiveAction()
        {
            return _actionState != FPSActionState.None;
        }

        private bool IsAiming()
        {
            return _aimState is FPSAimState.Aiming or FPSAimState.PointAiming;
        }

        private void InitializeMovement()
        {
            _movementComponent = GetComponent<CustomFPSMovement>();
            
            _movementComponent.OnJumping = () => { PlayTransitionMotion(_settings.jumpingMotion); };
            _movementComponent.OnLanded = () => { PlayTransitionMotion(_settings.jumpingMotion); };

            _movementComponent.OnCrouching = OnCrouch;
            _movementComponent.OnUncrouch = OnUncrouch;

            _movementComponent.OnSprintStarted = OnSprintStarted;
            _movementComponent.OnSprintEnded = OnSprintEnded;

            _movementComponent.OnSlideStarted = OnSlideStarted;

            _movementComponent.SlideActionCondition += () => !HasActiveAction();
            _movementComponent.SprintActionCondition += () => !HasActiveAction();
            _movementComponent.ProneActionCondition += () => !HasActiveAction();
            
            _movementComponent.OnStopMoving = () =>
            {
                PlayTransitionMotion(_settings.stopMotion);
            };
            
            _movementComponent.OnProneEnded = () =>
            {
                _userInput.SetValue(FPSANames.PlayablesWeight, 1f);
            };
        }

        private void UnequipWeapon()
        {
            DisableAim();
            _actionState = FPSActionState.WeaponChange;
            _weaponController.OnUnEquipWeapon();
        }

        private void EquipWeapon()
        {
            _weaponController.EquipWeapon();
            _actionState = FPSActionState.None;
        }

        private void DisableAim()
        {
            if (_weaponController.GetActiveItem().OnAimReleased()) 
                _aimState = FPSAimState.None;
        }
        
        private void OnFirePressed()
        { 
            if (!_weaponController.HasWeapon() || HasActiveAction()) 
                return;
            
            _weaponController.GetActiveItem().OnFirePressed();
        }

        private void OnFireReleased()
        {
            if (!_weaponController.HasWeapon()) 
                return;
            
            _weaponController.GetActiveItem().OnFireReleased();
        }
        
        private void OnSlideStarted()
        {
            _animator.CrossFade(SlideHash, 0.2f);
        }
        
        private void OnSprintStarted()
        {
            OnFireReleased();
            DisableAim();

            _aimState = FPSAimState.None;

            _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
            _userInput.SetValue("LookLayerWeight", 0.3f);
        }

        private void OnSprintEnded()
        {
            _userInput.SetValue(FPSANames.StabilizationWeight, 1f);
            _userInput.SetValue("LookLayerWeight", 1f);
        }

        private void OnCrouch()
        {
            PlayTransitionMotion(_settings.crouchingMotion);
        }

        private void OnUncrouch()
        {
            PlayTransitionMotion(_settings.crouchingMotion);
        }
        
        private void UpdateLookInput()
        {
            float scale = _userInput.GetValue<float>(_sensitivityMultiplierPropertyIndex);
            
            float deltaMouseX = _lookDeltaInput.x * _settings.sensitivity * scale;
            float deltaMouseY = -_lookDeltaInput.y * _settings.sensitivity * scale;
            
            _playerInput.y += deltaMouseY;
            _playerInput.x += deltaMouseX;
            
            if (_recoilPattern)
            {
                _playerInput += _recoilPattern.GetRecoilDelta();
                deltaMouseX += _recoilPattern.GetRecoilDelta().x;
            }
            
            float proneWeight = _animator.GetFloat(ProneWeightHash);
            Vector2 pitchClamp = Vector2.Lerp(new Vector2(-90f, 90f), new Vector2(-30, 0f), proneWeight);

            _playerInput.y = Mathf.Clamp(_playerInput.y, pitchClamp.x, pitchClamp.y);
            
            transform.rotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
            
            _userInput.SetValue(FPSANames.MouseDeltaInput, new Vector4(deltaMouseX, deltaMouseY));
            _userInput.SetValue(FPSANames.MouseInput, new Vector4(_playerInput.x, _playerInput.y));
        }

        private void OnMovementUpdated()
        {
            float playablesWeight = 1f - _animator.GetFloat(FullBodyWeightHash);
            _userInput.SetValue(FPSANames.PlayablesWeight, playablesWeight);
        }

#if ENABLE_INPUT_SYSTEM
        public void OnReload()
        {
            if (IsSprinting() || HasActiveAction() || !_weaponController.GetActiveItem().OnReload()) 
                return;
            
            _actionState = FPSActionState.PlayingAnimation;
        }

        public void OnThrowGrenade()
        {
            if (IsSprinting()|| HasActiveAction() || !_weaponController.GetActiveItem().OnGrenadeThrow()) 
                return;
            
            _actionState = FPSActionState.PlayingAnimation;
        }
        
        public void OnFire(InputValue value)
        {
            if (IsSprinting()) 
                return;
            
            if (value.isPressed)
            {
                OnFirePressed();
                return;
            }
            
            OnFireReleased();
        }

        public void OnAim(InputValue value)
        {
            if (IsSprinting()) 
                return;

            bool isPressed = value.isPressed;
            if (isPressed && !IsAiming())
            {
                if (_weaponController.GetActiveItem().OnAimPressed()) 
                    _aimState = FPSAimState.Aiming;
                
                PlayTransitionMotion(_settings.aimingMotion);
            }
            else if (isPressed && IsAiming())
            {
                DisableAim();
                PlayTransitionMotion(_settings.aimingMotion);
            }
        }

        public void OnLook(InputValue value)
        {
            _lookDeltaInput = value.Get<Vector2>();
        }

        public void OnLean(InputValue value)
        {
            _userInput.SetValue(FPSANames.LeanInput, value.Get<float>() * _settings.leanAngle);
            PlayTransitionMotion(_settings.leanMotion);
        }

        public void OnCycleScope()
        {
            if (!IsAiming())
                return;
            
            _weaponController.GetActiveItem().OnCycleScope();
            PlayTransitionMotion(_settings.aimingMotion);
        }

        public void OnChangeFireMode()
        {
            _weaponController.GetActiveItem().OnChangeFireMode();
        }

        public void OnToggleAttachmentEditing()
        {
            if (HasActiveAction() && _actionState != FPSActionState.AttachmentEditing)
                return;
            
            _actionState = _actionState == FPSActionState.AttachmentEditing 
                ? FPSActionState.None : FPSActionState.AttachmentEditing;

            if (_actionState == FPSActionState.AttachmentEditing)
            {
                _animator.CrossFade(InspectStartHash, 0.2f);
                return;
            }
            
            _animator.CrossFade(InspectEndHash, 0.3f);
        }

        public void OnDigitAxis(InputValue value)
        {
            if (!value.isPressed || _actionState != FPSActionState.AttachmentEditing) 
                return;
            
            _weaponController.GetActiveItem().OnAttachmentChanged((int) value.Get<float>());
        }
#endif
    }
}