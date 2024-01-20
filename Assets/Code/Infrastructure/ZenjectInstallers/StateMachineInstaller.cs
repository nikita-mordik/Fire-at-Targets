using FreedLOW.FireAtTargets.Code.Infrastructure.State;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class StateMachineInstaller : Installer<StateMachineInstaller>
    {
        public override void InstallBindings()
        {
            BindGameStateMachine();
            BindStateFactory();
        }

        private void BindGameStateMachine()
        {
            Container.Bind<IStateMachine>()
                .To<StateMachine>()
                .AsSingle()
                .NonLazy();
        }

        private void BindStateFactory()
        {
            Container.Bind<IStateFactory>()
                .To<StateFactory>()
                .AsSingle()
                .NonLazy();
        }
    }
}