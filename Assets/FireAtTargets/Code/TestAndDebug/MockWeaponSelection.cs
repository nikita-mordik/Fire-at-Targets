using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Weapon;
using FreedLOW.FireAtTargets.Code.StaticData;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.TestAndDebug
{
    public class MockWeaponSelection : MonoBehaviour
    {
        public SelectedWeaponMock SelectedWeaponMock;
        
        private ISelectWeaponService _selectWeaponService;

        [Inject]
        private void Construct(ISelectWeaponService selectWeaponService)
        {
            _selectWeaponService = selectWeaponService;
        }

        private void Start()
        {
            _selectWeaponService.SelectWeapon(SelectedWeaponMock.SelectedWeapon);
        }
    }
}