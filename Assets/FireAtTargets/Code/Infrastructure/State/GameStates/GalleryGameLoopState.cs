using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class GalleryGameLoopState : IState
    {
        private readonly IPrefabPoolService _poolService;
        private readonly IGameFactory _gameFactory;

        public GalleryGameLoopState(IPrefabPoolService poolService, IGameFactory gameFactory)
        {
            _poolService = poolService;
            _gameFactory = gameFactory;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await UniTask.Yield();
        }

        public void Exit()
        {
            _poolService.CleanUp();
            _gameFactory.CleanUp();
        }
    }
}