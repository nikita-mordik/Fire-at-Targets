using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader;
using FreedLOW.FireAtTargets.Code.Infrastructure.State;
using FreedLOW.FireAtTargets.Code.Infrastructure.Time;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindUnityTimeService();
            BindSceneLoaderService();
            BindGameStateMachine();
            BindPlayer();
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
    }
}