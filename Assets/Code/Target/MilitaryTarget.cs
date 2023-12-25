using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Target
{
    public class MilitaryTarget : MonoBehaviour , IMilitaryTarget
    {
        [SerializeField] private Transform root;
        [SerializeField] private TargetShootPointType targetShootPointType;
        
        private ITargetHealth targetHealth;

        public TargetShootPointType TargetShootPointType => targetShootPointType;

        private void Awake()
        {
            targetHealth = root.GetComponent<ITargetHealth>();
            Debug.LogError(targetHealth!=null);
        }

        public void Damage(int damageAmount)
        {
            Debug.LogError("here hit " + targetShootPointType);
            targetHealth.TakeDamage(damageAmount);
            
            // TODO: send point data using TargetShootPointType
            //pointService.AddPoint(targetShootPointType);
        }
    }
}