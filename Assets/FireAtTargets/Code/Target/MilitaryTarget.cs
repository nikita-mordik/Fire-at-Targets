using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Target
{
    public class MilitaryTarget : MonoBehaviour, IMilitaryTarget
    {
        [SerializeField] private Transform root;
        [SerializeField] private TargetShootPointType targetShootPointType;
        
        private ITargetHealth _targetHealth;
        
        public TargetShootPointType TargetShootPointType => targetShootPointType;

        private IPointService _pointService;

        [Inject]
        private void Construct(IPointService pointService)
        {
            _pointService = pointService;
        }

        private void Awake()
        {
            _targetHealth = root.GetComponent<ITargetHealth>();
        }

        public void Damage(int damageAmount)
        {
            // A downed (or still recovering) target must not keep giving points.
            if (!_targetHealth.IsAlive)
                return;

            _targetHealth.TakeDamage(damageAmount);
            if (!_targetHealth.IsAlive)
            {
                _pointService.AddExtraPoints(targetShootPointType);
            }
            else
            {
                _pointService.AddPoint(targetShootPointType);
            }
        }
    }
}