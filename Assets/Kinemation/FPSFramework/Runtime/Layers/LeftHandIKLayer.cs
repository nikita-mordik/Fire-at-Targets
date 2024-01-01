// Designed by KINEMATION, 2023


using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;

using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public struct BoneRef
    {
        public Transform bone;
        public Quaternion rotation;
        
        public BoneRef(Transform boneRef)
        {
            bone = boneRef;
            rotation = Quaternion.identity;
        }
        
        public void CopyBone(bool localSpace = true)
        {
            if (localSpace)
            {
                rotation = bone.localRotation;
                return;
            }

            rotation = bone.rotation;
        }
        
        public void Slerp(float weight, bool localSpace = true)
        {
            if (localSpace)
            {
                bone.localRotation = Quaternion.Slerp(bone.localRotation, rotation, weight);
                return;
            }
            
            bone.rotation = Quaternion.Slerp(bone.rotation, rotation, weight);
        }
        
        public static void InitBoneChain(ref List<BoneRef> chain, Transform parent, AvatarMask mask)
        {
            if (chain == null || mask == null || parent == null) return;
            
            chain.Clear();
            for (int i = 1; i < mask.transformCount; i++)
            {
                if (mask.GetTransformActive(i))
                {
                    var t = parent.Find(mask.GetTransformPath(i));
                    chain.Add(new BoneRef(t));
                }
            }
        }
    }
    
    public class LeftHandIKLayer : AnimLayer
    {
        [Header("Left Hand IK Settings")]

        [AnimCurveName] [SerializeField] private string maskCurveName;
        [SerializeField] private AvatarMask leftHandMask;
        [SerializeField] private bool usePoseOverride = true;
        [SerializeField] private bool forceLeftHandUpdate = true;

        private LocRot _leftHandPose = LocRot.identity;
        private LocRot _leftHandPoseCache = LocRot.identity;
        
        private List<BoneRef> _leftHandChain = new List<BoneRef>();

        public override void InitializeLayer()
        {
            base.InitializeLayer();
            
            if (leftHandMask == null)
            {
                Debug.LogWarning("LeftHandIKLayer: no mask for the left hand assigned!");
                return;
            }

            BoneRef.InitBoneChain(ref _leftHandChain, transform, leftHandMask);
        }

        public override void OnPoseSampled()
        {
            _leftHandPoseCache = _leftHandPose;
            
            Quaternion rotOffset = GetGunAsset().rotationOffset;
            LocRot weaponPivot = new LocRot(GetRigData().weaponBone);
            weaponPivot.rotation *= rotOffset;

            weaponPivot.position += weaponPivot.rotation * GetPivotPoint().localPosition;
            weaponPivot.rotation *= GetPivotPoint().localRotation;
            
            _leftHandPose = GetTransforms().leftHandTarget == null
                ? new LocRot(GetLeftHandIK().target).ToSpace(weaponPivot)
                : new LocRot(GetTransforms().leftHandTarget).ToSpace(GetPivotPoint());
            
            if (!usePoseOverride || leftHandMask == null)
            {
                return;
            }
            
            for (int i = 0; i < _leftHandChain.Count; i++)
            {
                var bone = _leftHandChain[i];
                bone.CopyBone();
                _leftHandChain[i] = bone;
            }
        }

        private void OverrideLeftHand(float weight)
        {
            weight = Mathf.Clamp01(weight);

            if (Mathf.Approximately(weight, 0f))
            {
                return;
            }
            
            foreach (var bone in _leftHandChain)
            {
                bone.Slerp(weight);
            }
        }
        
        public override void PreUpdateLayer()
        {
            base.PreUpdateLayer();
            smoothLayerAlpha = layerAlpha * (1f - GetCurveValue(maskCurveName)) * (1f - smoothLayerAlpha);
        }
        
        public override void UpdateLayer()
        {
            if (forceLeftHandUpdate && GetTransforms().leftHandTarget != null)
            {
                _leftHandPose = new LocRot(GetTransforms().leftHandTarget).ToSpace(GetPivotPoint());
            }
            
            if (usePoseOverride)
            {
                OverrideLeftHand(smoothLayerAlpha);
            }
            
            float progress = core.animGraph.GetPoseProgress();
            LocRot blendedPose = LocRot.Lerp(_leftHandPoseCache, _leftHandPose, progress);
            blendedPose = blendedPose.FromSpace(GetMasterPivot());
            GetLeftHandIK().Override(blendedPose, smoothLayerAlpha);
        }
    }
}