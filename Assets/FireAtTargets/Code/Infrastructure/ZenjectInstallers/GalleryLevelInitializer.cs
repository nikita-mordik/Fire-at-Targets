using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class GalleryLevelInitializer : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform uiRoot;
        
        private IGameFactory _gameFactory;
        private IPrefabPoolService _poolService;

        [Inject]
        private void Construct(IGameFactory gameFactory, IPrefabPoolService poolService)
        {
            _gameFactory = gameFactory;
            _poolService = poolService;
        }
        
        public async void Initialize()
        {
            InitializeHUD();

            // FOR TEST, THEN SHOULD DELETE THIS:
            await _poolService.InitializePoolAsync(AssetLabel.ShootingGalleryPool);
        }
        
        private async void InitializeHUD()
        {
            var hud = await _gameFactory.CreateHUD();
            hud.transform.SetParent(uiRoot);
        }
    }
}