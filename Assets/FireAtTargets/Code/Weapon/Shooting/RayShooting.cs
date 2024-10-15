using FreedLOW.FireAtTargets.Code.Target;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Weapon.Shooting
{
    public class RayShooting : MonoBehaviour
    {
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float maxDistance;
        
        [Header("Effects")]
        [SerializeField] private ShootHitEffect shootHitEffect;
        [SerializeField] private ShootAudio shootAudio;

        private readonly RaycastHit[] _hits = new RaycastHit[1];
        
        private int _damageAmount;

        public int DamageAmount { set => _damageAmount = value; }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(shootPoint.position, shootPoint.forward * maxDistance);
        }

        public void OnFire()
        {
            shootAudio.PlayShoot();
            shootHitEffect.ShootEffect(shootPoint.position);
            
            var hitCount = Physics.RaycastNonAlloc(shootPoint.position, shootPoint.forward, _hits, maxDistance);
            if (hitCount <= 0)
                return;
            
            if (_hits[0].collider.TryGetComponent<IMilitaryTarget>(out var militaryTarget))
            {
                militaryTarget.Damage(_damageAmount);
            }

            shootAudio.PlayHit(_hits[0], shootPoint.forward);
            shootHitEffect.HitEffect(_hits[0]);
        }
    }
}