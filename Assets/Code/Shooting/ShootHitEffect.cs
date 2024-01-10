using System.Linq;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Shooting
{
    public class ShootHitEffect : MonoBehaviour
    {
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private ImpactInfo[] impactElemets;

        public void ShootEffect(Vector3 shootPointPosition)
        {
            var impactEffectInstance = Instantiate(muzzleFlash, shootPointPosition, transform.rotation);
            Destroy(impactEffectInstance, 4f);
        }

        public void HitEffect(RaycastHit hit)
        {
            var effect = GetImpactEffect(hit.transform.gameObject);
            if (effect == null)
                return;
            
            var effectInstance = Instantiate(effect, hit.point, new Quaternion());
            effectInstance.transform.LookAt(hit.point + hit.normal);
            Destroy(effectInstance, 15f);
        }

        private GameObject GetImpactEffect(GameObject impactedGameObject)
        {
            var materialType = impactedGameObject.GetComponent<MaterialType>();
            if (materialType == null)
                return null;
            
            return (from impactInfo in impactElemets where impactInfo.MaterialType == materialType.TypeOfMaterial select impactInfo.ImpactEffect)
                .FirstOrDefault();
        }
    }
}