// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.CollisionLayer
{
    public class CollisionLayerState : FPSAnimatorLayerState
    {
        private CollisionLayerSettings _settings;
        private Transform _weaponIkBone;
        private KTransform _blockingPose = KTransform.Identity;

        private int _mouseInputPropertyIndex;

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (CollisionLayerSettings) newSettings;
            _weaponIkBone = _rigComponent.GetRigTransform(_settings.weaponIkBone);

            _mouseInputPropertyIndex = _inputController.GetPropertyIndex(FPSANames.MouseInput);
        }

        public override void OnEvaluatePose()
        {
            Vector3 direction = _weaponIkBone.forward;
            Vector3 start = _weaponIkBone.position - direction * _settings.rayStartOffset;

            bool bHit = Physics.Raycast(start, direction, out var hit, _settings.barrelLength, 
                _settings.layerMask);
            
            KTransform target = KTransform.Identity;
            if (bHit)
            {
                float blockRatio = 1f - hit.distance / (_settings.barrelLength + _settings.rayStartOffset);
                
                float mouseInput = _inputController.GetValue<Vector4>(_mouseInputPropertyIndex).y;
                target = KTransform.Lerp(target, 
                    mouseInput > 0f ? _settings.secondaryPose : _settings.primaryPose, blockRatio);
            }
            
            float alpha = KMath.ExpDecayAlpha(_settings.smoothingSpeed, Time.deltaTime);
            _blockingPose = KTransform.Lerp(_blockingPose, target, alpha);

            KPose pose = new KPose()
            {
                modifyMode = EModifyMode.Add,
                pose = _blockingPose,
                space = _settings.targetSpace
            };
            
            KAnimationMath.ModifyTransform(_owner.transform, _weaponIkBone, pose, Weight);
        }
    }
}