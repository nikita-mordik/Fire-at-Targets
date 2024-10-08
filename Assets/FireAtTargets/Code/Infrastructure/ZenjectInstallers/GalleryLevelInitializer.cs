using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class GalleryLevelInitializer : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform uiRoot;
        
        private IGameFactory _gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }
        
        public void Initialize()
        {
            InitializeHUD();
        }
        
        private async void InitializeHUD()
        {
            var hud = await _gameFactory.CreateHUD();
            hud.transform.SetParent(uiRoot);
        }
    }
}