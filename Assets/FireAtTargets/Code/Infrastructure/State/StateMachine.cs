using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State
{
    public class StateMachine : IStateMachine
    {
        private readonly IStateFactory stateFactory;

        private IExitableState activeState;

        public StateMachine(IStateFactory stateFactory)
        {
            this.stateFactory = stateFactory;
        }

        public async UniTask Enter<TState>(Action action = null) where TState : class, IState
        {
            activeState = ChangeState<TState>();
            await ((TState)activeState).Enter(action);
        }

        public async UniTask Enter<TState, TPayload>(TPayload payload, Action action = null) where TState : class, IPayloadedState<TPayload>
        {
            activeState = ChangeState<TState>();
            await ((TState)activeState).Enter(payload, action);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            activeState?.Exit();
            
            var currentState = stateFactory.Create<TState>();
            return currentState;
        }
    }
}