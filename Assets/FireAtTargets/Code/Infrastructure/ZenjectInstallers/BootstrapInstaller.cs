using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader;
using FreedLOW.FireAtTargets.Code.Infrastructure.State;
using FreedLOW.FireAtTargets.Code.Infrastructure.Time;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindSceneLoaderService();
            BindGameStateMachine();
            BindPlayer();
            BindAssetProvider();
            BindGameFactory();
            BindInputService();
            BindPoolService();
            BindUnityTimeService();
            BindWeaponHandlerService();
        }

        private void BindPoolService()
        {
            Container.Bind<IPrefabPoolService>()
                .To<PrefabPoolService>()
                .AsSingle();
        }

        private void BindGameFactory()
        {
            Container.Bind<IGameFactory>()
                .To<GameFactory>()
                .AsSingle();
        }

        private void BindAssetProvider()
        {
            Container.Bind<IAssetProvider>()
                .To<AssetProvider>()
                .AsSingle();
        }

        private void BindPlayer()
        {
            Container.Bind<IPlayerConstructor>()
                .To<PlayerConstructor>()
                .AsSingle();
        }

        private void BindGameStateMachine()
        {
            Container.Bind<IStateMachine>()
                .FromSubContainerResolve()
                .ByInstaller<StateMachineInstaller>()
                .AsSingle();
        }

        private void BindSceneLoaderService()
        {
            Container.Bind<ISceneLoaderService>()
                .To<SceneLoaderService>()
                .AsSingle();
        }

        private void BindUnityTimeService()
        {
            Container.Bind<ITimeService>()
                .To<UnityTimeService>()
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
                .FromMethod(GetInputService)
                .AsSingle();
        }

        private InputService GetInputService(InjectContext context)
        {
            if (Application.isEditor)
            {
                var assetProvider = context.Container.Resolve<IAssetProvider>();
                return new StandaloneInputService(assetProvider);
            }

            return new MobileShootingGalleryInputService();
        }
    }
}