using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using TMPro;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon.UI
{
    public class AmmoView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ammoText;

        private int _currentAmmo;
        private int _magazineCountAmmo;
        
        private IWeaponEventHandlerService _weaponEventHandlerService;

        [Inject]
        private void Construct(IWeaponEventHandlerService weaponEventHandlerService)
        {
            _weaponEventHandlerService = weaponEventHandlerService;
        }

        private void OnEnable()
        {
            _weaponEventHandlerService.OnCurrentAmmoChanged += OnCurrentAmmoChanged;
            _weaponEventHandlerService.OnReload += OnReload;
            _weaponEventHandlerService.OnWeaponEquip += OnWeaponEquip;
        }

        private void OnDisable()
        {
            _weaponEventHandlerService.OnCurrentAmmoChanged -= OnCurrentAmmoChanged;
            _weaponEventHandlerService.OnReload -= OnReload;
            _weaponEventHandlerService.OnWeaponEquip -= OnWeaponEquip;
        }

        private void OnCurrentAmmoChanged(int value)
        {
            _currentAmmo = value;
            UpdateAmmoView();
        }

        private void OnReload(int value)
        {
            _magazineCountAmmo = value;
            UpdateAmmoView();
        }
        
        private void OnWeaponEquip(CustomWeapon weapon)
        {
            _currentAmmo = weapon.CurrentAmmo;
            _magazineCountAmmo = weapon.MaxAmmo;
            UpdateAmmoView();
        }

        private void UpdateAmmoView()
        {
            _ammoText.text = $"{_currentAmmo} / {_magazineCountAmmo}";
        }
    }
}