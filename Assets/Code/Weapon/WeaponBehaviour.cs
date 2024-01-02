using System;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.Shooting;
using FreedLOW.FireAtTargets.Code.StaticData;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [SerializeField] private FPSAnimWeapon weapon;
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private RayShooting rayShooting;

        private int currentAmmo;
        private int currentMaxAmmo;

        public int MaxAmmo { get; private set; }
        public int StartAmmo { get; private set; }
        public int CurrentAmmo
        {
            get => currentAmmo;
            set
            {
                if (value < 0)
                    throw new Exception("Incorrect value!");

                currentAmmo = value;
                weaponHandlerService.InvokeOnCurrentAmmoChanged(currentAmmo);
            }
        }

        private IWeaponHandlerService weaponHandlerService;

        [Inject]
        private void Construct(IWeaponHandlerService weaponHandlerService)
        {
            this.weaponHandlerService = weaponHandlerService;
        }

        private void Awake()
        {
            weapon.fireRate = weaponData.FireRate;
            weapon.fireMode = weaponData.FireMode;
            weapon.burstAmount = weaponData.BurstAmount;
            rayShooting.DamageAmount = weaponData.Damage;
            MaxAmmo = weaponData.MaxAmmo;
            currentMaxAmmo = MaxAmmo;
            StartAmmo = weaponData.StartAmmo;
            CurrentAmmo = StartAmmo;
        }

        public void Reload()
        {
            CurrentAmmo = StartAmmo;
            currentMaxAmmo -= CurrentAmmo;
            
            // TODO: here invoke event OnReload
            weaponHandlerService.InvokeOnReload(currentMaxAmmo);
            Debug.LogError("here reload");
        }

        public bool HasMagazineAmmo() => 
            currentMaxAmmo > 0;
    }
}