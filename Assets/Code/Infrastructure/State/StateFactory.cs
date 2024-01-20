using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State
{
    public class StateFactory : IStateFactory
    {
        private IInstantiator instantiator;

        public StateFactory(IInstantiator instantiator)
        {
            this.instantiator = instantiator;
        }

        public TState Create<TState>() where TState : IExitableState => 
            instantiator.Instantiate<TState>();
    }
}