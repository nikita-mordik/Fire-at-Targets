using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class GalleryLevelInitializer : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform _uiRoot;
        
        private IGameFactory _gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }
        
        public void Initialize()
        {
            //InitializeHUD().Forget();
        }
        
        private async UniTaskVoid InitializeHUD()
        {
            var hud = await _gameFactory.CreateHUDAsync();
            hud.transform.SetParent(_uiRoot);
        }
    }
}