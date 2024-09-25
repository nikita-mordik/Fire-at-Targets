using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService
{
    public interface IPrefabPoolService
    {
        /// <summary>
        /// Initializing pool
        /// </summary>
        void InitializePoolAsync(string assetsLabel);
        /// <summary>
        /// Get gameObject from Pool
        /// </summary>
        /// <param name="type">Type of GO which will be create</param>
        /// <returns></returns>
        UniTask<GameObject> GetObjectFromPool(ObjectType type);
        /// <summary>
        /// Return gameObject to Pool
        /// </summary>
        /// <param name="gameObject">GO which will be return</param>
        void BackObjectToPool(GameObject gameObject);
        /// <summary>
        /// Clean resources
        /// </summary>
        void CleanUp();
    }
}