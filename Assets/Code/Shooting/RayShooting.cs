using FreedLOW.FireAtTergets.Code.Infrastructure.Services.Input;
using FreedLOW.FireAtTergets.Code.Target;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTergets.Code.Shooting
{
    public class RayShooting : MonoBehaviour
    {
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float maxDistance;
        [SerializeField] private LayerMask targetLayer;

        private readonly RaycastHit[] hits = new RaycastHit[1];
        
        private int damageAmount;

        public int DamageAmount { get => damageAmount; set => damageAmount = value; }
        
        private IInputService inputService;

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;

            this.inputService.OnShoot += OnShoot;
        }

        private void OnDestroy()
        {
            inputService.OnShoot -= OnShoot;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color=Color.magenta;
            Gizmos.DrawRay(shootPoint.position, shootPoint.forward * maxDistance);
        }

        private void OnShoot()
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