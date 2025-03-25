using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Weapon
{
    public interface ISelectWeaponService
    {
        GameObject SelectedWeapon { get; }
        void SelectWeapon(GameObject weapon);
    }
}