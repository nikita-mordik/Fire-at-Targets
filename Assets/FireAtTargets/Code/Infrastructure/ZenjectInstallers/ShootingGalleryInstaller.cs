using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class ShootingGalleryInstaller : MonoInstaller
    {
        public PlayerControllerService PlayerControllerService;
        
        public override void InstallBindings()
        {
            BindWeaponHandlerService();
            BindPointService();
            BindPlayerControllerService();
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
    }
}