// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.WeaponLayer
{
    public abstract class WeaponLayerState : FPSAnimatorLayerState
    {
        private WeaponLayerSettings _weaponSettings;

        protected Transform _weaponIkBone;
        private Transform _rightHandHint;
        private Transform _leftHandHint;

        private KTransform _cachedRightHandHint;
        private KTransform _cachedLeftHandHint;

        private void CacheHintTargets()
        {
            _cachedRightHandHint.position = _rightHandHint.position;
            _cachedRightHandHint.rotation = _rightHandHint.rotation;
            
            _cachedLeftHandHint.position = _leftHandHint.position;
            _cachedLeftHandHint.rotation = _leftHandHint.rotation;
        }

        private void ApplyHintTargets()
        {
            float weight = _weaponSettings.hintTargetWeight;
            
            _rightHandHint.position = Vector3.Lerp(_cachedRightHandHint.position, _rightHandHint.position, weight);
            _rightHandHint.rotation = Quaternion.Slerp(_cachedRightHandHint.rotation, _rightHandHint.rotation, weight);
            
            _leftHandHint.position = Vector3.Lerp(_cachedLeftHandHint.position, _leftHandHint.position, weight);
            _leftHandHint.rotation = Quaternion.Slerp(_cachedLeftHandHint.rotation, _leftHandHint.rotation, weight);
        }

        // Use this method to animate bones.
        protected virtual void OnEvaluateWeaponPose()
        {
        }
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _weaponSettings = (WeaponLayerSettings) newSettings;

            _weaponIkBone = _rigComponent.GetRigTransform(_weaponSettings.weaponIkBone);
            _rightHandHint = _rigComponent.GetRigTransform(_weaponSettings.rightHandElbow);
            _leftHandHint = _rigComponent.GetRigTransform(_weaponSettings.leftHandElbow);
        }

        public override void OnEvaluatePose()
        {
            KTransform weaponIkPose = new KTransform(_weaponIkBone);
            
            CacheHintTargets();
            OnEvaluateWeaponPose();
            ApplyHintTargets();

            var localOffset = weaponIkPose.GetRelativeTransform(new KTransform(_weaponIkBone), false);

            Vector3 offset = _weaponSettings.animatedPivotOffset;
            offset = localOffset.rotation * offset - offset;

            KAnimationMath.MoveInSpace(_weaponIkBone, _weaponIkBone, offset, Weight);
        }
    }
}