using FreedLOW.FireAtTergets.Code.Shooting;
using FreedLOW.FireAtTergets.Code.StaticData;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Weapon
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [SerializeField] private FPSAnimWeapon weapon;
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private RayShooting rayShooting;

        private void Awake()
        {
            weapon.fireRate = weaponData.FireRate;
            weapon.fireMode = weaponData.FireMode;
            weapon.burstAmount = weaponData.BurstAmount;
            rayShooting.DamageAmount = weaponData.Damage;
        }
    }
}