using FreedLOW.FireAtTargets.Code.Infrastructure.State;
using FreedLOW.FireAtTargets.Code.Infrastructure.State.GameStates;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.UI.Menu
{
    public class MenuWindow : MonoBehaviour
    {
        [SerializeField] private Button shootingGalleryButton;
        
        private IStateMachine stateMachine;

        [Inject]
        private void Construct(IStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
        
        private void Start()
        {
            shootingGalleryButton.onClick.AddListener(OnPlayShootingGallery);
        }

        private async void OnPlayShootingGallery()
        {
            await stateMachine.Enter<LoadGalleryState>();
        }
    }
}