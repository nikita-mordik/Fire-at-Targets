using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State
{
    public interface IStateMachine
    {
        UniTask Enter<TState>(Action action = null) where TState : class, IState;
        UniTask Enter<TState, TPayload>(TPayload payload, Action action = null) where TState : class, IPayloadedState<TPayload>;
    }
}