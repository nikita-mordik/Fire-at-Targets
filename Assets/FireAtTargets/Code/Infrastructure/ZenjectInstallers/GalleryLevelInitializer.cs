using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class GalleryLevelInitializer : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform uiRoot;
        
        private IGameFactory gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            this.gameFactory = gameFactory;
        }
        
        public void Initialize()
        {
            InitializeHUD();
        }
        
        private async void InitializeHUD()
        {
            var hud = await gameFactory.CreateHUD();
            hud.transform.SetParent(uiRoot);
        }
    }
}