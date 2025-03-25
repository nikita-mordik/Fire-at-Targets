using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class CharacterInstaller : MonoInstaller
    {
        public PlayerControllerService PlayerControllerService;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerControllerService>()
                .FromInstance(PlayerControllerService)
                .AsSingle()
                .NonLazy();

            PlayerControllerService.Initialize();
        }
    }
}