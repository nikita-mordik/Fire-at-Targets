using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using TMPro;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon.UI
{
    public class AmmoView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ammoText;

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
        }

        private void OnDisable()
        {
            _weaponEventHandlerService.OnCurrentAmmoChanged -= OnCurrentAmmoChanged;
            _weaponEventHandlerService.OnReload -= OnReload;
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

        private void UpdateAmmoView()
        {
            ammoText.text = $"{_currentAmmo} / {_magazineCountAmmo}";
        }
    }
}