using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Extensions;
using FreedLOW.FireAtTargets.Code.Infrastructure.Factory;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon.Shooting
{
    public class ShootHitEffect : MonoBehaviour
    {
        [SerializeField] private ImpactInfo[] impactElemets;
        
        private IGameFactory gameFactory;
        private IPrefabPoolService poolService;

        [Inject]
        private void Construct(IGameFactory gameFactory, IPrefabPoolService poolService)
        {
            this.poolService = poolService;
            this.gameFactory = gameFactory;
        }

        public async void ShootEffect(Vector3 shootPointPosition)
        {
            var impactEffectInstance = await gameFactory.CreateShootParticle(ObjectType.MuzzleFlashOne);
            impactEffectInstance.transform.SetTransform(shootPointPosition, transform.rotation);
            await BackEffectToPoolRoutine(impactEffectInstance, 4f);
        }

        public async void HitEffect(RaycastHit hit)
        {
            var effect = GetImpactEffect(hit.transform.gameObject);
            if (effect == null)
                return;

            var poolObject = effect.GetComponent<IPoolObject>();
            var effectInstance = await gameFactory.CreateHitParticle(poolObject.Type);
            effectInstance.transform.SetTransform(hit.point, Quaternion.identity);
            //Instantiate(effect, hit.point, new Quaternion());
            effectInstance.transform.LookAt(hit.point + hit.normal);
            await BackEffectToPoolRoutine(effectInstance, 15f);
        }

        private GameObject GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType == null)
                return null;
            
            return (from impactInfo in impactElemets where impactInfo.MaterialType == materialType.TypeOfMaterial select impactInfo.ImpactEffect)
                .FirstOrDefault();
        }
        
        private async UniTask BackEffectToPoolRoutine(GameObject impactEffectInstance, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            poolService.BackObjectToPool(impactEffectInstance);
        }
    }
}