using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class MenuState : IState
    {
        public async UniTask Enter(Action action = null)
        {
            await UniTask.Yield();
        }

        public void Exit()
        {
            
        }
    }
}