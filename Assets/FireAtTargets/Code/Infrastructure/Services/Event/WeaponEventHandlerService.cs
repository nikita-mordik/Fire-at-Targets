using System;
using FreedLOW.FireAtTargets.Code.Weapon;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event
{
    public class WeaponEventHandlerService : IWeaponEventHandlerService
    {
        public event Action<int> OnCurrentAmmoChanged;
        public event Action<int> OnReload;
        public event Action<CustomWeapon> OnWeaponEquip;

        public void InvokeOnCurrentAmmoChanged(int currentAmmo) => 
            OnCurrentAmmoChanged?.Invoke(currentAmmo);

        public void InvokeOnReload(int currentMaxAmmo) => 
            OnReload?.Invoke(currentMaxAmmo);

        public void InvokeOnWeaponEquip(CustomWeapon weapon) => 
            OnWeaponEquip?.Invoke(weapon);
    }
}