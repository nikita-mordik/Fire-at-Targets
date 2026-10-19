using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.State;
using FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates;
using FreedLOW.FireAtTargets.Code.Save;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        private IStateMachine stateMachine;
        private IAssetProvider assetProvider;
        private ISaveService saveService;

        [Inject]
        private void Construct(IStateMachine stateMachine, IAssetProvider assetProvider, ISaveService saveService)
        {
            this.stateMachine = stateMachine;
            this.assetProvider = assetProvider;
            this.saveService = saveService;
        }

        private async void Awake()
        {
            saveService.Load();
            await assetProvider.InitializeAsync();

            await stateMachine.Enter<LoadMenuState>();
        }
    }
}