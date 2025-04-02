using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assetProvider;
        private readonly IInstantiator _instantiator;
        private readonly IPrefabPoolService _poolService;

        private GameObject _characterGameObject;
        private GameObject _hudGameObject;

        public GameFactory(IAssetProvider assetProvider, IInstantiator instantiator, IPrefabPoolService poolService)
        {
            _assetProvider = assetProvider;
            _instantiator = instantiator;
            _poolService = poolService;
        }
        
        public async UniTask<GameObject> CreatePlayerAsyncAt(Transform at)
        {
            var asset = await _assetProvider.LoadAsset(AssetName.Character);
            _characterGameObject = InstantiateInjectObject(asset);
            IPlayerControllerService playerControllerService = _characterGameObject.GetComponent<IPlayerControllerService>();
            playerControllerService.Initialize();
            playerControllerService.SetPositionAndRotation(at);
            return _characterGameObject;
        }

        public async UniTask<GameObject> CreateHUDAsync()
        {
            var asset = await _assetProvider.LoadAsset(AssetName.HUDLabel);
            _hudGameObject = InstantiateInjectObject(asset);
            return _hudGameObject;
        }

        public async UniTask<GameObject> CreateShootParticleAsync(ObjectType muzzleType) => 
            await _poolService.GetObjectFromPool(muzzleType);

        public async UniTask<GameObject> CreateHitParticleAsync(ObjectType hitType) => 
            await _poolService.GetObjectFromPool(hitType);

        public void CleanUp()
        {
            Object.Destroy(_characterGameObject);
            Object.Destroy(_hudGameObject);
        }

        private static GameObject InstantiateObject(GameObject asset) => 
            Object.Instantiate(asset);

        private static GameObject InstantiateObjectWithPosition(GameObject asset, Vector3 position, Quaternion rotation) => 
            Object.Instantiate(asset, position, rotation);

        private GameObject InstantiateInjectObject(GameObject asset) => 
            _instantiator.InstantiatePrefab(asset);
    }
}