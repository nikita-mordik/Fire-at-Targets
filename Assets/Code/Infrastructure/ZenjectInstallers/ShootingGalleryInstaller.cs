using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class ShootingGalleryInstaller : MonoInstaller
    {
        public PlayerControllerService PlayerControllerService;
        
        public override void InstallBindings()
        {
            BindInputService();
            BindWeaponHandlerService();
            BindPointService();
            BindPlayerConstructorService();
            BindPlayerControllerService();
        }

        private void BindPlayerConstructorService()
        {
            Container.Bind<IPlayerConstructor>()
                .To<PlayerConstructor>()
                .AsSingle();
        }

        private void BindPlayerControllerService()
        {
            Container.Bind<IPlayerControllerService>()
                .To<PlayerControllerService>()
                .FromComponentInHierarchy(PlayerControllerService)
                .AsSingle();

            // Container.Bind<IPlayerControllerService>()
            //     .To<PlayerControllerService>()
            //     .FromComponentInNewPrefab(PlayerControllerService)
            //     .AsSingle();
        }

        private void BindPointService()
        {
            Container.Bind<IPointService>()
                .To<PointService>()
                .AsSingle();
        }

        private void BindWeaponHandlerService()
        {
            Container.Bind<IWeaponEventHandlerService>()
                .To<WeaponEventHandlerService>()
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