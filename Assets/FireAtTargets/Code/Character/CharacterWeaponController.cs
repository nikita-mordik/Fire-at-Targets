using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Demo.Scripts.Runtime.Character;
using Demo.Scripts.Runtime.Item;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Weapon;
using FreedLOW.FireAtTargets.Code.Weapon;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Character
{
    public class CharacterWeaponController : MonoBehaviour
    {
        [SerializeField] private FPSControllerSettings _settings;

        private List<FPSItem> _instantiatedWeapons;
        private Transform _weaponBone;

        private ISelectWeaponService _selectWeaponService;
        private IInstantiator _instantiator;
        private IWeaponEventHandlerService _weaponEventService;
        private IAssetProvider _assetProvider;

        [Inject]
        private void Construct(ISelectWeaponService selectWeaponService, IInstantiator instantiator,
            IWeaponEventHandlerService weaponEventService, IAssetProvider assetProvider)
        {
            _selectWeaponService = selectWeaponService;
            _instantiator = instantiator;
            _weaponEventService = weaponEventService;
            _assetProvider = assetProvider;
        }

        public async void Initialize()
        {
            await SetupWeapon();
            EquipWeapon();
        }

        public FPSItem GetActiveItem()
        {
            return _instantiatedWeapons.Count == 0 ? null : _instantiatedWeapons[0];
        }

        public void EquipWeapon()
        {
            if (_instantiatedWeapons.Count == 0)
            {
                Debug.LogError("No weapons has instantiated");
                return;
            }
            
            _instantiatedWeapons[0].gameObject.SetActive(true);
            _instantiatedWeapons[0].OnEquip(gameObject);
            _weaponEventService.InvokeOnWeaponEquip(_instantiatedWeapons[0].GetComponent<CustomWeapon>());
        }

        public void OnUnEquipWeapon()
        {
            _instantiatedWeapons[0].OnUnEquip();
        }

        public bool HasWeapon()
        {
            return _instantiatedWeapons.Count > 0;
        }

        private async UniTask SetupWeapon()
        {
            _instantiatedWeapons = new List<FPSItem>();
            _weaponBone = GetComponentInChildren<KRigComponent>().GetRigTransform(_settings.weaponBone);

            var prefab = await _assetProvider.LoadAsset(_selectWeaponService.SelectedWeapon.AssetGUID);
            GameObject weaponObject = _instantiator.InstantiatePrefab(prefab);
            var weaponTransform = weaponObject.transform;
            weaponTransform.parent = _weaponBone;
            weaponTransform.localPosition = Vector3.zero;
            weaponTransform.localRotation = Quaternion.identity;
            
            _instantiatedWeapons.Add(weaponObject.GetComponent<FPSItem>());
            weaponObject.SetActive(false);
        }
    }
}