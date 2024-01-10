using System;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event
{
    public class WeaponEventHandlerService : IWeaponEventHandlerService
    {
        public event Action<int> OnCurrentAmmoChanged;
        public event Action<int> OnReload;
        
        public void InvokeOnCurrentAmmoChanged(int currentAmmo) => 
            OnCurrentAmmoChanged?.Invoke(currentAmmo);

        public void InvokeOnReload(int currentMaxAmmo) => 
            OnReload?.Invoke(currentMaxAmmo);
    }
}