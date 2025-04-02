using System.Linq;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Extensions;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using Impact.Interactions.Decals;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon.Shooting
{
    public class ShootHitEffect : MonoBehaviour
    {
        [SerializeField] private ImpactInfo[] impactElemets;
        
        private const float ShootEffectDelay = 4f;
        private const float HitEffectDelay = 5f;
        private const float DecalDelay = 6f;

        private IGameFactory _gameFactory;
        private IPrefabPoolService _poolService;

        [Inject]
        private void Construct(IGameFactory gameFactory, IPrefabPoolService poolService)
        {
            _poolService = poolService;
            _gameFactory = gameFactory;
        }

        public async void ShootEffect(Vector3 shootPointPosition)
        {
            var impactEffectInstance = await _gameFactory.CreateShootParticleAsync(ObjectType.MuzzleFlashOne);
            impactEffectInstance.transform.SetTransform(shootPointPosition, transform.rotation);
            await BackEffectToPoolWithDelay(impactEffectInstance, ShootEffectDelay);
        }

        public async void HitEffect(RaycastHit hit)
        {
            var effect = GetImpactEffect(hit.transform.gameObject);
            if (effect == null)
                return;

            var poolObject = effect.GetComponent<IPoolObject>();
            var effectInstance = await _gameFactory.CreateHitParticleAsync(poolObject.Type);
            effectInstance.transform.SetTransform(hit.point, Quaternion.identity);
            effectInstance.transform.LookAt(hit.point + hit.normal);
            await BackEffectToPoolWithDelay(effectInstance, HitEffectDelay);
            await BackAllDecalsToPoolWithDelay(hit);
        }

        private GameObject GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType == null)
                return null;

            return (from impactInfo in impactElemets
                    where impactInfo.MaterialType == materialType.TypeOfMaterial
                    select impactInfo.ImpactEffect)
                .FirstOrDefault();
        }
        
        private async UniTask BackEffectToPoolWithDelay(GameObject impactEffectInstance, float delay)
        {
            await UniTask.WaitForSeconds(delay);
            _poolService.BackObjectToPool(impactEffectInstance);
        }
        
        private async UniTask BackAllDecalsToPoolWithDelay(RaycastHit hit)
        {
            await UniTask.WaitForSeconds(DecalDelay);
            if (hit.transform.childCount > 0)
            {
                foreach (Transform child in hit.transform)
                {
                    child.GetComponent<ImpactDecal>()?.MakeAvailable();
                }
            }
        }
    }
}