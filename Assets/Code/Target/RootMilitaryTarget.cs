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

        public void ShootDown(Action action)
        {
            transform.DORotate(shootDownRotation, shootDownDuration)
                .OnComplete(() => action?.Invoke());
        }

        public void RecoveryTarget(Action action)
        {
            transform.DORotateQuaternion(initialRotation, recoveryDuration)
                .OnComplete(() => action?.Invoke());
        }
    }
}