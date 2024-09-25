using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Factory
{
    public interface IGameFactory
    {
        UniTask<GameObject> CreatePlayer();
        UniTask<GameObject> CreateHUD();
        UniTask<GameObject> CreateShootParticle(ObjectType muzzleTyp);
        UniTask<GameObject> CreateHitParticle(ObjectType hitType);
        void CleanUp();
    }
}