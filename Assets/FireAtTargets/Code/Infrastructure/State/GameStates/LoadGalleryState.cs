using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Common;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class LoadGalleryState : IState
    {
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IStateMachine _stateMachine;
        private readonly IPrefabPoolService _poolService;
        private readonly IGameFactory _gameFactory;
        
        private GameObject _root;

        public LoadGalleryState(ISceneLoaderService sceneLoaderService, IStateMachine stateMachine,
            IPrefabPoolService poolService, IGameFactory gameFactory)
        {
            _sceneLoaderService = sceneLoaderService;
            _stateMachine = stateMachine;
            _poolService = poolService;
            _gameFactory = gameFactory;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await _sceneLoaderService.LoadSceneAsync(SceneName.GalleryScene, () => OnLoad(action));
        }

        public void Exit() { }

        private async void OnLoad(Action action)
        {
            _root = GameObject.FindWithTag(Tags.SceneRoot);

            await InitializeHUD();
            await InitializeCharacter();
            await InitializePool();
            
            action?.Invoke();
            await _stateMachine.Enter<GalleryGameLoopState>();
        }

        private async UniTask InitializeHUD()
        {
            var hud = await _gameFactory.CreateHUDAsync();
            hud.transform.SetParent(_root.transform);
        }
        
        private async UniTask InitializeCharacter()
        {
            var player = await _gameFactory.CreatePlayerAsync();
            player.transform.SetParent(_root.transform);
        }

        private async UniTask InitializePool()
        {
            await _poolService.InitializePoolAsync(AssetLabel.ShootingGalleryPool);
        }
    }
}