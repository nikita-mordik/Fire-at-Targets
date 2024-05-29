// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Layers.WeaponLayer;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Input;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.AdsLayer
{
    public class AdsLayerState : WeaponLayerState
    {
        private FPSAnimatorEntity _entity;
        private AdsLayerSettings _settings;
        
        private Transform _aimTargetBone;
        private Transform _cachedAimPoint;

        private float _aimingWeight;
        private KTransform _additivePose = KTransform.Identity;
        
        private KTransform _aimPoint = KTransform.Identity;
        private KTransform _prevAimPoint = KTransform.Identity;
        private float _aimPointPlayback;

        private int _isAimingPropertyIndex;

        // Returns an additive ads pose in Component space.
        private KTransform GetAdsPose()
        {
            KTransform componentBone =
                new KTransform(_owner.transform).GetRelativeTransform(new KTransform(_weaponIkBone), false);
            
            KTransform aimBone =
                new KTransform(_owner.transform).GetRelativeTransform(new KTransform(_aimTargetBone), false);
            
            KTransform result = new KTransform()
            {
                position = aimBone.position - componentBone.position,
                rotation = Quaternion.Inverse(componentBone.rotation)
            };

            return result;
        }

        private Vector3 GetEuler(Quaternion rotation)
        {
            Vector3 result = rotation.eulerAngles;

            result.x = KMath.NormalizeEulerAngle(result.x);
            result.y = KMath.NormalizeEulerAngle(result.y);
            result.z = KMath.NormalizeEulerAngle(result.z);

            return result;
        }

        private KTransform GetLocalAimPoint(Transform aimPoint)
        {
            KTransform result = KTransform.Identity;
            
            result.rotation = Quaternion.Inverse(_weaponIkBone.rotation) * aimPoint.rotation;
            result.position = -_weaponIkBone.InverseTransformPoint(aimPoint.position);

            return result;
        }

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            base.InitializeState(newSettings);
            
            _settings = (AdsLayerSettings) newSettings;
            _aimTargetBone = _rigComponent.GetRigTransform(_settings.aimTargetBone);

            _additivePose = GetAdsPose();

            _isAimingPropertyIndex = _inputController.GetPropertyIndex(_settings.isAimingProperty);
        }

        public override void OnEntityUpdated(FPSAnimatorEntity newEntity)
        {
            _entity = newEntity;
            _aimPointPlayback = 1f;
            _cachedAimPoint = _entity.defaultAimPoint;
            _aimPoint = _prevAimPoint = GetLocalAimPoint(_cachedAimPoint);
        }

        public override void OnGameThreadUpdate()
        {
            bool isAiming = _inputController.GetValue<bool>(_isAimingPropertyIndex);

            _aimingWeight += _settings.aimingSpeed * (isAiming ? Time.deltaTime : -Time.deltaTime);
            _aimingWeight = Mathf.Clamp01(_aimingWeight);
            
            _inputController.SetValue(FPSANames.AimingWeight, _aimingWeight);
            _aimPointPlayback = Mathf.Clamp01(_aimPointPlayback + Time.deltaTime * _settings.aimPointSpeed);
        }

        protected override void OnEvaluateWeaponPose()
        {
            if (_entity == null) return;
            
            if (_entity.defaultAimPoint != _cachedAimPoint)
            {
                _aimPointPlayback = 0f;
                _prevAimPoint = _aimPoint;
            }
            
            float weight = KCurves.Ease(0f, 1f, _aimingWeight, _settings.aimingEaseMode) * _settings.alpha;
            
            AdsBlend blend = _settings.positionBlend;
            KTransform pose = GetAdsPose();

            pose.position.x = Mathf.Lerp(pose.position.x, _additivePose.position.x, blend.x);
            pose.position.y = Mathf.Lerp(pose.position.y, _additivePose.position.y, blend.y);
            pose.position.z = Mathf.Lerp(pose.position.z, _additivePose.position.z, blend.z);

            blend = _settings.rotationBlend;
            Vector3 absQ = GetEuler(pose.rotation);
            Vector3 addQ = GetEuler(_additivePose.rotation);

            absQ.x = Mathf.Lerp(absQ.x, addQ.x, blend.x);
            absQ.y = Mathf.Lerp(absQ.y, addQ.y, blend.y);
            absQ.z = Mathf.Lerp(absQ.z, addQ.z, blend.z);

            pose.rotation = Quaternion.Euler(absQ);

            KAnimationMath.RotateInSpace(_owner.transform, _weaponIkBone, pose.rotation, weight);
            KAnimationMath.MoveInSpace(_owner.transform, _weaponIkBone, pose.position, weight);

            KTransform aimPoint = GetLocalAimPoint(_entity.defaultAimPoint);
            _aimPoint = KTransform.EaseLerp(_prevAimPoint, aimPoint, _aimPointPlayback, _settings.aimingEaseMode);

            Quaternion socketRotation = _aimPoint.rotation;
            Vector3 socketPosition = _aimPoint.position;

            KAnimationMath.MoveInSpace(_owner.transform, _weaponIkBone, socketRotation * socketPosition, weight);
            KAnimationMath.RotateInSpace(_owner.transform, _weaponIkBone, socketRotation, weight);
            
            _cachedAimPoint = _entity.defaultAimPoint;
        }
    }
}