using FreedLOW.FireAtTergets.Code.Target;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Shooting
{
    public class RayShooting : MonoBehaviour
    {
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float maxDistance;
        [SerializeField] private LayerMask targetLayer;

        private readonly RaycastHit[] hits = new RaycastHit[1];
        
        private int damageAmount;

        private void FixedUpdate()
        {
            var hitCount = Physics.RaycastNonAlloc(shootPoint.position, shootPoint.forward, hits, maxDistance, targetLayer);
            if (hitCount > 0)
            {
                var militaryTarget = hits[0].collider.GetComponent<IMilitaryTarget>();
                militaryTarget.Damage(damageAmount);
            }
        }
    }
}