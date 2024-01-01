// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class AdsLayer : AnimLayer
    {
        [Header("SightsAligner")] [SerializeField]
        private EaseMode adsEaseMode = new EaseMode(EEaseFunc.Sine);

        [SerializeField] private EaseMode pointAimEaseMode = new EaseMode(EEaseFunc.Sine);
        [SerializeField] [Bone] protected Transform aimTarget;

        [SerializeField] private LocRot crouchPose = LocRot.identity;
        [SerializeField] [AnimCurveName(true)] private string crouchPoseCurve;

        protected bool bAds;
        protected float adsProgress;
        
        protected bool bPointAim;
        protected float pointAimProgress;
        
        protected float adsWeight;
        protected float pointAimWeight;
        
        protected LocRot interpAimPoint = LocRot.identity;
        protected LocRot targetAimPoint = LocRot.identity;
        protected LocRot viewOffsetCache = LocRot.identity;
        
        protected LocRot additiveAds = LocRot.identity;
        protected LocRot absoluteAds = LocRot.identity;
        
        public void SetAds(bool bAiming)
        {
            bAds = bAiming;
            UpdateAimPoint();
            interpAimPoint = bAds ? targetAimPoint : interpAimPoint;
        }
        
        public void SetPointAim(bool bAiming)
        {
            bPointAim = bAiming;
        }

        public void UpdateAimPoint()
        {
            targetAimPoint = GetAdsOffset();
        }

        public override void OnPoseSampled()
        {
            if (GetGunAsset() == null) return;
            
            viewOffsetCache = GetGunAsset().viewOffset;
            
            // Refresh the Mater IK as it was the normal pose.
            LocRot weaponBoneMS = new LocRot(GetRigData().weaponBone);
            weaponBoneMS.rotation *= GetGunAsset().rotationOffset;
            
            GetMasterPivot().position = weaponBoneMS.position;
            GetMasterPivot().rotation = weaponBoneMS.rotation;
            
            GetMasterIK().Offset(GetPivotPoint().localPosition);
            GetMasterIK().Offset(GetPivotPoint().localRotation);
            
            LocRot masterCache = new LocRot(GetMasterPivot());
            
            // Apply absolute aiming to the base pose.
            GetMasterPivot().position = aimTarget.position;
            GetMasterPivot().rotation = GetRootBone().rotation;

            // Calculate the delta between Base Aim and Base Hip poses.
            LocRot masterMS = new LocRot(GetMasterPivot()).ToSpace(GetRootBone());
            LocRot pivotPoseMeshSpace = masterCache.ToSpace(GetRootBone());
            
            additiveAds.position = masterMS.position - pivotPoseMeshSpace.position;
            additiveAds.rotation = Quaternion.Inverse(pivotPoseMeshSpace.rotation) * masterMS.rotation;

            GetMasterPivot().position = masterCache.position;
            GetMasterPivot().rotation = masterCache.rotation;
        }

        public override void UpdateLayer()
        {
            if (GetAimPoint() == null)
            {
                OffsetViewModel(1f);
                return;
            }
            
            UpdateAimWeights();
            ApplyCrouchPose();
            ApplyPointAiming();
            ApplyAiming();
        }
        
        protected void UpdateAimWeights()
        {
            float adsRate = GetGunAsset().adsData.aimSpeed;
            float pointAimRate = GetGunAsset().adsData.pointAimSpeed;
            
            adsWeight = CurveLib.Ease(0f, 1f, adsProgress, adsEaseMode);
            pointAimWeight = CurveLib.Ease(0f, 1f, pointAimProgress, pointAimEaseMode);
            
            adsProgress += Time.deltaTime * (bAds ? adsRate : -adsRate);
            pointAimProgress += Time.deltaTime * (bPointAim ? pointAimRate : -pointAimRate);

            adsProgress = Mathf.Clamp(adsProgress, 0f, 1f);
            pointAimProgress = Mathf.Clamp(pointAimProgress, 0f, 1f);
            
            core.ikRigData.aimWeight = adsWeight;
        }

        protected LocRot GetAdsOffset()
        {
            LocRot adsOffset = new LocRot(Vector3.zero, Quaternion.identity);

            if (GetAimPoint() == null)
            {
                return adsOffset;
            }
            
            adsOffset.rotation = Quaternion.Inverse(GetPivotPoint().rotation) * GetAimPoint().rotation;
            adsOffset.position = -GetPivotPoint().InverseTransformPoint(GetAimPoint().position);

            return adsOffset;
        }

        protected void AlignSights(float weight)
        {
            if (Mathf.Approximately(weight, 0f)) return;
            
            // 1. Compute the offsets for absolute and additive methods.
            absoluteAds = ComputeAbsoluteOffset();
            LocRot adsOffset = LocRot.identity;
            
            // 2. Get the blending values.
            var positionBlend = GetGunAsset().adsData.adsTranslationBlend;
            var rotationBlend = GetGunAsset().adsData.adsRotationBlend;

            // 3. Blend translation.
            adsOffset.position.x = Mathf.Lerp(absoluteAds.position.x, additiveAds.position.x, positionBlend.x);
            adsOffset.position.y = Mathf.Lerp(absoluteAds.position.y, additiveAds.position.y, positionBlend.y);
            adsOffset.position.z = Mathf.Lerp(absoluteAds.position.z, additiveAds.position.z, positionBlend.z);

            Vector3 eulerAbsolute = CoreToolkitLib.ToEuler(absoluteAds.rotation);
            Vector3 eulerAdditive = CoreToolkitLib.ToEuler(additiveAds.rotation);
            
            // 4. Blend rotation.
            eulerAbsolute.x = Mathf.Lerp(eulerAbsolute.x, eulerAdditive.x, rotationBlend.x);
            eulerAbsolute.y = Mathf.Lerp(eulerAbsolute.y, eulerAdditive.y, rotationBlend.y);
            eulerAbsolute.z = Mathf.Lerp(eulerAbsolute.z, eulerAdditive.z, rotationBlend.z);

            adsOffset.rotation = Quaternion.Euler(eulerAbsolute);
            
            // 5. Align master IK.
            GetMasterIK().Offset(GetRootBone(), adsOffset.rotation, weight);
            GetMasterIK().Offset(GetRootBone(), adsOffset.position, weight);
            
            // 6. Apply aim point offset.
            GetMasterIK().Offset(GetRootBone(), interpAimPoint.rotation * interpAimPoint.position, weight);
            GetMasterIK().Offset(GetRootBone(), interpAimPoint.rotation, weight);
        }

        protected virtual void ApplyAiming()
        {
            interpAimPoint = CoreToolkitLib.Interp(interpAimPoint, targetAimPoint, 
                GetGunAsset().adsData.changeSightSpeed, Time.deltaTime);
            
            float aimWeight = Mathf.Clamp01(adsWeight - pointAimWeight);
            
            OffsetViewModel(1f - aimWeight);
            AlignSights(aimWeight);
        }

        protected void ApplyCrouchPose()
        {
            float poseAlpha = GetAnimator().GetFloat(crouchPoseCurve) * (1f - adsWeight);
            if (Mathf.Approximately(poseAlpha, 0f)) return;
            
            GetMasterIK().Offset(GetRootBone(), crouchPose.position, poseAlpha);
            GetMasterIK().Offset(GetRootBone().rotation, crouchPose.rotation, poseAlpha);
        }

        protected virtual void ApplyPointAiming()
        {
            if (Mathf.Approximately(pointAimWeight, 0f)) return;
            
            var pointAimOffset = GetGunAsset().adsData.pointAimOffset;
            GetMasterIK().Offset(GetRootBone(), pointAimOffset.position, pointAimWeight);
            GetMasterIK().Offset(GetRootBone(), pointAimOffset.rotation, pointAimWeight);
        }

        protected virtual void OffsetViewModel(float weight)
        {
            if (Mathf.Approximately(weight, 0f)) return;
            
            float poseProgress = core.animGraph.GetPoseProgress();
            var viewOffset = LocRot.Lerp(viewOffsetCache, GetGunAsset().viewOffset, poseProgress);
            
            GetMasterIK().Offset(GetRootBone(), viewOffset.position, weight);
            GetMasterIK().Offset(GetRootBone(), viewOffset.rotation, weight);
        }
        
        // Absolute aiming overrides base animation
        protected virtual LocRot ComputeAbsoluteOffset()
        {
            LocRot pivotGlobal = new LocRot(GetMasterPivot()).ToSpace(GetRootBone());

            LocRot absoluteOffset = pivotGlobal;
            absoluteOffset.position = GetRootBone().InverseTransformPoint(aimTarget.position);
            absoluteOffset.position -= pivotGlobal.position;
            absoluteOffset.rotation = Quaternion.Inverse(pivotGlobal.rotation);
            
            return absoluteOffset;
        }
    }
}