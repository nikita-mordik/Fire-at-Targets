using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class GalleryGameLoopState : IState
    {
        private readonly IPrefabPoolService poolService;

        public GalleryGameLoopState(IPrefabPoolService poolService)
        {
            this.poolService = poolService;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await UniTask.Yield();
        }

        public void Exit()
        {
            poolService.CleanUp();
        }
    }
}