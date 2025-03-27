using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class GalleryLevelInitializer : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform _uiRoot;
        
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
            InitializeHUD().Forget();

            // FOR TEST, THEN SHOULD DELETE THIS:
            //await _poolService.InitializePoolAsync(AssetLabel.ShootingGalleryPool);
        }
        
        private async UniTaskVoid InitializeHUD()
        {
            var hud = await _gameFactory.CreateHUD();
            hud.transform.SetParent(_uiRoot);
        }
    }
}