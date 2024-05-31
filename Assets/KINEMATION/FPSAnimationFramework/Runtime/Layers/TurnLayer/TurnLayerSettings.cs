// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Attributes;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.TurnLayer
{
    public class TurnLayerSettings : FPSAnimatorLayerSettings
    {
        public KRigElement characterRootBone;
        [Range(0f, 90f)] public float angleThreshold = 90f;

        public AnimationCurve turnCurve;
        [Min(0f)] public float turnSpeed = 0f;
        
        [InputProperty] public string mouseDeltaInputProperty = FPSANames.MouseDeltaInput;
        [InputProperty] public string turnInputProperty;

        public string animatorTurnRightTrigger;
        public string animatorTurnLeftTrigger;

        public override FPSAnimatorLayerState CreateState()
        {
            return new TurnLayerState();
        }

#if UNITY_EDITOR
        public override void OnRigUpdated()
        {
            base.OnRigUpdated();
            UpdateRigElement(ref characterRootBone);
        }
#endif
    }
}
