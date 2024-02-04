using System;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates
{
    public class LoadMenuState : IState
    {
        private readonly ISceneLoaderService sceneLoaderService;
        private readonly IStateMachine stateMachine;

        public LoadMenuState(ISceneLoaderService sceneLoaderService, IStateMachine stateMachine)
        {
            this.sceneLoaderService = sceneLoaderService;
            this.stateMachine = stateMachine;
        }
        
        public async UniTask Enter(Action action = null)
        {
            await sceneLoaderService.LoadSceneAsync(SceneName.MenuScene, () => OnLoad(action));
        }

        public void Exit()
        {
            
        }

        private async void OnLoad(Action action)
        {
            action?.Invoke();
            await stateMachine.Enter<MenuState>();
        }
    }
}