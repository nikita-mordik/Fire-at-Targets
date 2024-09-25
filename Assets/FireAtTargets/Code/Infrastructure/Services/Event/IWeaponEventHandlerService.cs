using System;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event
{
    public interface IWeaponEventHandlerService
    {
        event Action<int> OnCurrentAmmoChanged;
        event Action<int> OnReload;
        
        void InvokeOnCurrentAmmoChanged(int currentAmmo);
        void InvokeOnReload(int currentMaxAmmo);
    }
}