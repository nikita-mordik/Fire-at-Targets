using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Target
{
    public class MilitaryTarget : MonoBehaviour , IMilitaryTarget
    {
        [SerializeField] private Transform root;
        [SerializeField] private TargetShootPointType targetShootPointType;
        
        private ITargetHealth targetHealth;

        public TargetShootPointType TargetShootPointType => targetShootPointType;

        private IPointService pointService;

        [Inject]
        private void Construct(IPointService pointService)
        {
            this.pointService = pointService;
        }

        private void Awake()
        {
            targetHealth = root.GetComponent<ITargetHealth>();
        }

        public void Damage(int damageAmount)
        {
            Debug.LogError("here hit " + targetShootPointType);
            targetHealth.TakeDamage(damageAmount);
            
            // TODO: send point data using TargetShootPointType
            pointService.AddPoint(targetShootPointType);
        }
    }
}