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
        private readonly ISceneLoaderService sceneLoaderService;
        private readonly IStateMachine stateMachine;
        private readonly IGameFactory gameFactory;
        private readonly IPrefabPoolService poolService;

        public LoadGalleryState(ISceneLoaderService sceneLoaderService, IStateMachine stateMachine,
            IGameFactory gameFactory, IPrefabPoolService poolService)
        {
            this.sceneLoaderService = sceneLoaderService;
            this.stateMachine = stateMachine;
            this.gameFactory = gameFactory;
            this.poolService = poolService;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await sceneLoaderService.LoadSceneAsync(SceneName.GalleryScene, () => OnLoad(action));
        }

        public void Exit() { }

        private async void OnLoad(Action action)
        {
            InitializeHUD();
            InitializePool();
            
            action?.Invoke();
            await stateMachine.Enter<GalleryGameLoopState>();
        }

        private async void InitializeHUD()
        {
            var hud = await gameFactory.CreateHUD();
            var tag = GameObject.FindGameObjectWithTag(Tags.Root);
            hud.transform.SetParent(tag.transform);
        }

        private void InitializePool() => 
            poolService.InitializePoolAsync(AssetLabel.ShootingGalleryPool);
    }
}