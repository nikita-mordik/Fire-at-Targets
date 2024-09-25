using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService
{
    public class PoolObject : MonoBehaviour, IPoolObject
    {
        [SerializeField] private ObjectType objectType;
        public ObjectType Type => objectType;
    }
}