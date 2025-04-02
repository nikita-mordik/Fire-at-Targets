using System;
using FreedLOW.FireAtTargets.Code.Weapon;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event
{
    public interface IWeaponEventHandlerService
    {
        event Action<int> OnCurrentAmmoChanged;
        event Action<int> OnReload;
        event Action<CustomWeapon> OnWeaponEquip; 
        
        void InvokeOnCurrentAmmoChanged(int currentAmmo);
        void InvokeOnReload(int currentMaxAmmo);
        void InvokeOnWeaponEquip(CustomWeapon weapon);
    }
}