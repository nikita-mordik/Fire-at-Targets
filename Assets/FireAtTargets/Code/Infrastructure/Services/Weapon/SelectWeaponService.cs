using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Weapon
{
    public class SelectWeaponService : ISelectWeaponService
    {
        public GameObject SelectedWeapon { get; private set; }

        public void SelectWeapon(GameObject weapon)
        {
            SelectedWeapon = weapon;
        }
    }
}