using System.Collections;
using FreedLOW.FireAtTargets.Code.Features.GameSystem;
using UnityEngine;
using Zenject;

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

        private IFireAtTargetsSystem _fireAtTargetsSystem;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => _isAlive;

        [Inject]
        private void Construct(IFireAtTargetsSystem fireAtTargetsSystem)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
        }

        private void Awake()
        {
            SetHealthData();
        }

        private void Start()
        {
            _fireAtTargetsSystem.OnGameStarted += ResetTarget;
            _fireAtTargetsSystem.OnGameCompleted += OnGameCompleted;
        }

        private void OnDestroy()
        {
            _fireAtTargetsSystem.OnGameStarted -= ResetTarget;
            _fireAtTargetsSystem.OnGameCompleted -= OnGameCompleted;
        }

        public void TakeDamage(int damage)
        {
            if (!_isAlive)
                return;

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                _isAlive = false;
                ShootDownTarget();
            }
        }

        /// <summary>
        /// Restores full health and stands the target back up if it is down, cancelling any pending recovery.
        /// </summary>
        public void ResetTarget()
        {
            StopAllCoroutines();

            if (_isAlive)
            {
                SetHealthData();
                return;
            }

            rootMilitaryTarget.RecoveryTarget(onComplete: SetHealthData);
        }

        private void OnGameCompleted(GameResult gameResult) => ResetTarget();

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
