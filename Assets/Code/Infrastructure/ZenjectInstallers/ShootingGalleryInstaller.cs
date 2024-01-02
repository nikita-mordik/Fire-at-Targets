using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class ShootingGalleryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindInputService();
            BindWeaponHandlerService();
            BindPointService();
        }

        private void BindPointService()
        {
            Container.Bind<IPointService>()
                .To<PointService>()
                .AsSingle();
        }

        private void BindWeaponHandlerService()
        {
            Container.Bind<IWeaponHandlerService>()
                .To<WeaponHandlerService>()
                .AsSingle();
        }

        private void BindInputService()
        {
            Container.Bind<IInputService>()
                .To<InputService>()
                .FromInstance(GetInputService())
                .AsSingle();
        }

        private static InputService GetInputService()
        {
            if (Application.isEditor)
                return new StandaloneInputService();

            return new MobileShootingGalleryInputService();
        }
    }
}