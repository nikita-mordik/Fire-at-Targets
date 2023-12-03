using System.Collections;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Target
{
    public class RootMilitaryTargetHealth : MonoBehaviour, ITargetHealth
    {
        [SerializeField] private int maxHealth;

        [Header("Components")]
        [SerializeField] private RootMilitaryTarget rootMilitaryTarget;
        
        private bool isAlive;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }

        private void Awake()
        {
            SetHealthData();
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;

            if (CurrentHealth <= 0 && isAlive)
            {
                isAlive = false;
                ShootDownTarget();
            }
        }

        private void SetHealthData()
        {
            CurrentHealth = maxHealth;
            isAlive = true;
        }

        private void ShootDownTarget() => 
            rootMilitaryTarget.ShootDown(() => StartCoroutine(RecoveryTargetRoutine()));

        private IEnumerator RecoveryTargetRoutine()
        {
            yield return new WaitForSeconds(5f);
            rootMilitaryTarget.RecoveryTarget(SetHealthData);
        }
    }
}