using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.State;
using FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        private IStateMachine stateMachine;
        private IAssetProvider assetProvider;

        [Inject]
        private void Construct(IStateMachine stateMachine, IAssetProvider assetProvider)
        {
            this.stateMachine = stateMachine;
            this.assetProvider = assetProvider;
        }

        private async void Awake()
        {
            await assetProvider.InitializeAsync();

            await stateMachine.Enter<LoadMenuState>();
        }
    }
}