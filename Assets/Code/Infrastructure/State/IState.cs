using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State
{
    public interface IState : IExitableState
    {
        UniTask Enter(Action action = null);
    }

    public interface IExitableState
    {
        void Exit();
    }
    
    public interface IPayloadedState<TPayload> : IExitableState
    {
        UniTask Enter(TPayload payload, Action action = null);
    }
}