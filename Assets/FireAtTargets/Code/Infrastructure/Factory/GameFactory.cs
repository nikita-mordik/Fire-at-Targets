using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Extensions;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider assetProvider;
        private readonly IInstantiator instantiator;
        private readonly IPrefabPoolService poolService;

        public GameFactory(IAssetProvider assetProvider, IInstantiator instantiator, IPrefabPoolService poolService)
        {
            this.assetProvider = assetProvider;
            this.instantiator = instantiator;
            this.poolService = poolService;
        }
        
        public UniTask<GameObject> CreatePlayer()
        {
            throw new System.NotImplementedException();
        }

        public async UniTask<GameObject> CreateHUD()
        {
            var asset = await assetProvider.LoadAsset(AssetName.HUDLabel);
            return InstantiateInjectObject(asset);
        }

        public async UniTask<GameObject> CreateShootParticle(ObjectType muzzleType) => 
            await poolService.GetObjectFromPool(muzzleType);

        public async UniTask<GameObject> CreateHitParticle(ObjectType hitType) => 
            await poolService.GetObjectFromPool(hitType);

        public void CleanUp()
        {
            throw new System.NotImplementedException();
        }

        private static GameObject InstantiateObject(GameObject asset) => 
            Object.Instantiate(asset);

        private static GameObject InstantiateObjectWithPosition(GameObject asset, Vector3 position, Quaternion rotation) => 
            Object.Instantiate(asset, position, rotation);

        private GameObject InstantiateInjectObject(GameObject asset) => 
            instantiator.InstantiatePrefab(asset);
    }
}