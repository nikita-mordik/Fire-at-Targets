using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using FreedLOW.FireAtTargets.Code.StaticData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService
{
    public class PrefabPoolService : IPrefabPoolService
    {
        private readonly IAssetProvider assetProvider;
        private readonly IInstantiator instantiator;
        
        private readonly List<PoolObjectInfo> listOfPoolObjects = new();
        private readonly List<string> currentLabels = new();

        private Dictionary<ObjectType, Pool> pools;
        private GameObject poolContainer;

        public PrefabPoolService(IAssetProvider assetProvider, IInstantiator instantiator)
        {
            this.assetProvider = assetProvider;
            this.instantiator = instantiator;
        }
        
        public async void InitializePoolAsync(string assetsLabel)
        {
            currentLabels.Add(assetsLabel);
            List<string> assetsListByLabel = await assetProvider.GetAssetsListByLabel<PoolObjectInfo>(assetsLabel);
            foreach (var asset in assetsListByLabel)
            {
                var loadAsset = await assetProvider.Load<PoolObjectInfo>(asset);
                listOfPoolObjects.Add(loadAsset);
            }

            if (pools is null)
                pools = new Dictionary<ObjectType, Pool>();
            
            var emptyGO = new GameObject();
            poolContainer ??= new GameObject("Pool");
            
            foreach (var objectInfo in listOfPoolObjects)
            {
                if (objectInfo.ObjectCount <= 0) continue;
                
                var container = Object.Instantiate(emptyGO, poolContainer.transform, false);
                container.name = objectInfo.ObjectType.ToString();

                pools[objectInfo.ObjectType] = new Pool(container.transform);

                for (int i = 0; i < objectInfo.ObjectCount; i++)
                {
                    var go = await InstantiateObject(listOfPoolObjects, objectInfo.ObjectType, container.transform);
                    pools[objectInfo.ObjectType].Objects.Enqueue(go);
                }
            }
            
            Object.Destroy(emptyGO);
        }

        public async UniTask<GameObject> GetObjectFromPool(ObjectType type)
        {
            if (!pools.TryGetValue(type, out var pool)) 
                return null;

            var obj = pool.Objects.Count > 0
                ? pool.Objects.Dequeue()
                : await InstantiateObject(listOfPoolObjects, type, pools[type].Container);
            obj.SetActive(true);
            return obj;
        }

        public void BackObjectToPool(GameObject gameObject)
        {
            pools[gameObject.GetComponent<IPoolObject>().Type].Objects.Enqueue(gameObject);
            gameObject.transform.position = pools[gameObject.GetComponent<IPoolObject>().Type].Container.position;
            gameObject.SetActive(false);
        }

        public async void CleanUp()
        {
            foreach (var currentLabel in currentLabels)
            {
                await assetProvider.ReleaseAssetsByLabel(currentLabel);
            }
            
            currentLabels.Clear();
            Object.Destroy(poolContainer);
            pools.Clear();
            listOfPoolObjects.Clear();
        }

        private async UniTask<GameObject> InstantiateObject(List<PoolObjectInfo> poolObjectInfos, ObjectType type, Transform parent)
        {
            AssetReference objectPrefab = poolObjectInfos.Find(x => x.ObjectType == type).ObjectPrefab;
            var prefab = await assetProvider.Load<GameObject>(objectPrefab);
            return instantiator.InstantiatePrefab(prefab, parent);
        }
    }
}