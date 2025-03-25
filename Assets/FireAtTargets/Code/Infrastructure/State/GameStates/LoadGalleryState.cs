using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class LoadGalleryState : IState
    {
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IStateMachine _stateMachine;
        private readonly IPrefabPoolService _poolService;

        public LoadGalleryState(ISceneLoaderService sceneLoaderService, IStateMachine stateMachine,
            IPrefabPoolService poolService)
        {
            _sceneLoaderService = sceneLoaderService;
            _stateMachine = stateMachine;
            _poolService = poolService;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await _sceneLoaderService.LoadSceneAsync(SceneName.GalleryScene, () => OnLoad(action));
        }

        public void Exit() { }

        private async void OnLoad(Action action)
        {
            await InitializePool();
            
            action?.Invoke();
            await _stateMachine.Enter<GalleryGameLoopState>();
        }

        private async UniTask InitializePool()
        {
            await _poolService.InitializePoolAsync(AssetLabel.ShootingGalleryPool);
        }
    }
}