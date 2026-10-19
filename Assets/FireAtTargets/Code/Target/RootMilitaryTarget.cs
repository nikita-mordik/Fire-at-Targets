using System;
using DG.Tweening;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Target
{
    public class RootMilitaryTarget : MonoBehaviour
    {
        [SerializeField] private float shootDownDuration;
        [SerializeField] private float recoveryDuration;
        [SerializeField] private Vector3 shootDownRotation = Vector3.zero;

        private Quaternion initialRotation;

        private void Awake()
        {
            initialRotation = transform.rotation;
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public void ShootDown(Action onComplete)
        {
            // Kill without completing, so an interrupted animation never fires its old callback.
            transform.DOKill();
            transform.DORotate(shootDownRotation, shootDownDuration)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void RecoveryTarget(Action onComplete)
        {
            transform.DOKill();
            transform.DORotateQuaternion(initialRotation, recoveryDuration)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
