using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Common;
using FreedLOW.FireAtTargets.Code.Extensions;
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
        
        private GameObject _hud;

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
            await _sceneLoaderService.LoadSceneAsync(AddressableKeys.ShootingGalleryAssets.ShootingGallery, () => OnLoad(action));
        }

        public void Exit()
        {
            CanvasGroup hudCanvasGroup = _hud.transform.GetChild(0).GetComponent<CanvasGroup>();
            hudCanvasGroup.DoState(true);
        }

        private async void OnLoad(Action action)
        {
            await InitializeHUD();
            await InitializeCharacterAt(GameObject.FindWithTag(Tags.StartPoint).transform);
            await InitializePool();
            
            action?.Invoke();
            await _stateMachine.Enter<GalleryGameLoopState>();
        }

        private async UniTask InitializeHUD()
        {
            _hud = await _gameFactory.CreateHUDAsync();
            _sceneLoaderService.MoveGameObjectToScene(_hud);
        }
        
        private async UniTask InitializeCharacterAt(Transform at)
        {
            var player = await _gameFactory.CreatePlayerAsyncAt(at);
            _sceneLoaderService.MoveGameObjectToScene(player);
        }

        private async UniTask InitializePool()
        {
            await _poolService.InitializePoolAsync(AssetLabel.ShootingGalleryPool);
        }
    }
}