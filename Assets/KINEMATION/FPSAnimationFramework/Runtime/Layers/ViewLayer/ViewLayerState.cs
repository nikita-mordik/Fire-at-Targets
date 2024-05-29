// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

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
    }
}