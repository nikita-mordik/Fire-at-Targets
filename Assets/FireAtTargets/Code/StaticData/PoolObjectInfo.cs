using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "PoolObject", menuName = "FireAtTargets/Pool")]
    public class PoolObjectInfo : ScriptableObject
    {
        public ObjectType ObjectType;
        public AssetReference ObjectPrefab;
        public int ObjectCount;
    }
}