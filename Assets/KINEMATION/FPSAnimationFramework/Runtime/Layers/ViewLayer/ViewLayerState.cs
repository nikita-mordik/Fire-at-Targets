// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.ViewLayer
{
    public class ViewLayerState : FPSAnimatorLayerState
    {
        private ViewLayerSettings _settings;

        private Transform _ikGunBone;
        private Transform _ikHandRight;
        private Transform _ikHandLeft;

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (ViewLayerSettings) newSettings;

            _ikGunBone = _rigComponent.GetRigTransform(_settings.ikHandGun.element);
            _ikHandRight = _rigComponent.GetRigTransform(_settings.ikHandRight.element);
            _ikHandLeft = _rigComponent.GetRigTransform(_settings.ikHandLeft.element);
            
            // Apply the transform for other layers.
            KAnimationMath.ModifyTransform(_owner.transform, _ikGunBone, _settings.ikHandGun);
        }

        public override void OnEvaluatePose()
        {
            Transform component = _owner.transform;
            
            KAnimationMath.ModifyTransform(component, _ikGunBone, _settings.ikHandGun, Weight);
            KAnimationMath.ModifyTransform(component, _ikHandRight, _settings.ikHandRight, Weight);
            KAnimationMath.ModifyTransform(component, _ikHandLeft, _settings.ikHandLeft, Weight);
        }

#if UNITY_EDITOR
        private Quaternion _rotationHandleCache = Quaternion.identity;
        
        public override void OnSceneGUI()
        {
            Transform space = _ikGunBone.transform;
            
            if (_settings.ikHandGun.space == ESpaceType.ParentBoneSpace)
            {
                space = _ikGunBone.parent;
            }

            if (_settings.ikHandGun.space == ESpaceType.ComponentSpace)
            {
                space = _owner.transform;
            }
            
            if (_settings.ikHandGun.space == ESpaceType.WorldSpace)
            {
                space = null;
            }
            
            Quaternion spaceRot = space == null ? Quaternion.identity : space.rotation;
            Vector3 handlePos = Handles.PositionHandle(_ikGunBone.position, spaceRot);

            if (space == null)
            {
                _settings.ikHandGun.pose.position += handlePos - _ikGunBone.position;
                return;
            }

            Vector3 boneLocal = space.InverseTransformPoint(_ikGunBone.position);
            handlePos = space.InverseTransformPoint(handlePos);

            _settings.ikHandGun.pose.position += handlePos - boneLocal;
        }
#endif
    }
}