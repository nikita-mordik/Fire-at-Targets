using System.Collections;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Target
{
    public class RootMilitaryTargetHealth : MonoBehaviour, ITargetHealth
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int minRecoveryDuration = 5;
        [SerializeField] private int maxRecoveryDuration = 15;

        [Header("Components")]
        [SerializeField] private RootMilitaryTarget rootMilitaryTarget;
        
        private bool _isAlive;
        private float _recoveryDuration;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }

        private void Awake()
        {
            SetHealthData();
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0 && _isAlive)
            {
                _isAlive = false;
                ShootDownTarget();
            }
        }

        private void SetHealthData()
        {
            CurrentHealth = maxHealth;
            _recoveryDuration = Random.Range(minRecoveryDuration, maxRecoveryDuration);
            _isAlive = true;
        }

        private void ShootDownTarget() => 
            rootMilitaryTarget.ShootDown(onComplete: () => StartCoroutine(RecoveryTargetRoutine()));

        private IEnumerator RecoveryTargetRoutine()
        {
            yield return new WaitForSeconds(_recoveryDuration);
            rootMilitaryTarget.RecoveryTarget(onComplete: SetHealthData);
        }
    }
}