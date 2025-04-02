using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Factory
{
    public interface IGameFactory
    {
        UniTask<GameObject> CreatePlayerAsyncAt(Transform at);
        UniTask<GameObject> CreateHUDAsync();
        UniTask<GameObject> CreateShootParticleAsync(ObjectType muzzleType);
        UniTask<GameObject> CreateHitParticleAsync(ObjectType hitType);
        void CleanUp();
    }
}