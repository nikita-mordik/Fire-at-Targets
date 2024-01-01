// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class LocomotionLayer : AnimLayer
    {
        [SerializeField] private float ikInterpolation = 0f;
        private LocRot _ikAdditive = LocRot.identity;
        
        private IKPose _ikPose = null;
        
        private LocRot _outIkPose = LocRot.identity;
        private LocRot _cachedIkPose = LocRot.identity;
        
        private float _ikPosePlayback;
        private float _blendSpeed;
        
        public override void PreUpdateLayer()
        {
            base.PreUpdateLayer();
            smoothLayerAlpha = 1f - smoothLayerAlpha * (1f - core.animGraph.GetCurveValue(CurveLib.Curve_Overlay));
            core.animGraph.SetGraphWeight(smoothLayerAlpha);
            core.ikRigData.weaponBoneWeight = GetCurveValue(CurveLib.Curve_WeaponBone);
        }
        
        public void BlendInIkPose(IKPose newPose)
        {
            if (newPose == null) return;
            
            _ikPose = newPose;
            _blendSpeed = _ikPose.blendInSpeed;
            _cachedIkPose = _outIkPose;
            _ikPosePlayback = 0f;
        }

        public void BlendOutIkPose(float blendOutSpeed = 0f)
        {
            _blendSpeed = _ikPose == null ? blendOutSpeed : _ikPose.blendOutSpeed;
            _ikPose = null;
            _cachedIkPose = _outIkPose;
            _ikPosePlayback = 0f;
        }

        private void UpdateIkPose()
        {
            _ikPosePlayback = CoreToolkitLib.Interp(_ikPosePlayback, 1f, _blendSpeed, Time.deltaTime);
            
            if (_ikPose == null)
            {
                _outIkPose = LocRot.Lerp(_cachedIkPose, LocRot.identity, _ikPosePlayback);
                return;
            }
            
            _outIkPose = LocRot.Lerp(_cachedIkPose, _ikPose.pose, _ikPosePlayback);
        }

        private void UpdateIkAdditive()
        {
            var additiveIkBone = GetRigData().weaponBoneAdditive;
            if (additiveIkBone == null) return;

            float alpha = CoreToolkitLib.ExpDecay(ikInterpolation, Time.deltaTime);
            _ikAdditive = LocRot.Lerp(_ikAdditive, new LocRot(additiveIkBone, false), alpha);
            
            Vector3 ikOffset = new Vector3()
            {
                x = GetCurveValue(CurveLib.Curve_IK_LeftHand_X),
                y = GetCurveValue(CurveLib.Curve_IK_LeftHand_Y),
                z = GetCurveValue(CurveLib.Curve_IK_LeftHand_Z),
            };
            
            GetLeftHandIK().Offset(GetMasterPivot(), ikOffset);
            
            ikOffset = new Vector3()
            {
                x = GetCurveValue(CurveLib.Curve_IK_X),
                y = GetCurveValue(CurveLib.Curve_IK_Y),
                z = GetCurveValue(CurveLib.Curve_IK_Z),
            };
            
            GetMasterIK().Offset(GetRootBone(), ikOffset);
        }

        public override void UpdateLayer()
        {
            UpdateIkPose();
            UpdateIkAdditive();
            
            GetMasterIK().Offset(GetRootBone(), _outIkPose.position + _ikAdditive.position, layerAlpha);
            GetMasterIK().Offset(GetRootBone(), _outIkPose.rotation * _ikAdditive.rotation, layerAlpha);
        }
    }
}