using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Common;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class LoadGalleryState : IState
    {
        private readonly SceneLoaderService sceneLoaderService;
        private readonly IStateMachine stateMachine;

        public LoadGalleryState(SceneLoaderService sceneLoaderService, IStateMachine stateMachine)
        {
            this.sceneLoaderService = sceneLoaderService;
            this.stateMachine = stateMachine;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await sceneLoaderService.LoadSceneAsync(SceneName.GalleryScene, () => OnLoad(action));
        }

        public void Exit()
        {
            
        }

        private void OnLoad(Action action)
        {
            action?.Invoke();
            stateMachine.Enter<GalleryGameLoopState>();
        }
    }
}