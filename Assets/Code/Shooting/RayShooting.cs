using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using FreedLOW.FireAtTargets.Code.Target;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Shooting
{
    public class RayShooting : MonoBehaviour
    {
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float maxDistance;
        [SerializeField] private LayerMask targetLayer;
        
        [Header("Effects")]
        [SerializeField] private ShootHitEffect shootHitEffect;
        [SerializeField] private ShootAudio shootAudio;

        private readonly RaycastHit[] hits = new RaycastHit[1];
        
        private int damageAmount;

        public int DamageAmount { set => damageAmount = value; }
        
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
            shootAudio.PlayShoot();
            var hitCount = Physics.RaycastNonAlloc(shootPoint.position, shootPoint.forward, hits, maxDistance);
            if (hitCount > 0)
            {
                if (hits[0].collider.TryGetComponent<IMilitaryTarget>(out var militaryTarget))
                {
                    militaryTarget.Damage(damageAmount);
                    shootHitEffect.HitMilitaryTargetEffect(hits[0].point);
                }
                else
                {
                    shootHitEffect.HitDefaultEffect(hits[0].point);
                }
            }
        }
    }
}