// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Camera;

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [Serializable]
    public struct FreeAimData
    {
        public float maxValue;
        [FormerlySerializedAs("speed")] public float interpolationSpeed;
        public float inputScale;
    }

    [Serializable]
    public struct MoveSwayData
    {
        [FormerlySerializedAs("maxMoveLocSway")] public Vector3 translationScale;
        [FormerlySerializedAs("maxMoveRotSway")] public Vector3 rotationScale;

        [FormerlySerializedAs("moveLocSway")] public VectorSpringData positionSpringSettings;
        [FormerlySerializedAs("moveRotSway")] public VectorSpringData rotationSpringSettings;

        [FormerlySerializedAs("locSpeed")] public float translationDampingFactor;
        [FormerlySerializedAs("rotSpeed")] public float rotationDampingFactor;
    }
    
    [Serializable]
    public struct GunBlockData
    {
        public float weaponLength;
        public float startOffset;
        public float threshold;
        public LocRot restPose;

        public GunBlockData(LocRot pose)
        {
            restPose = pose;
            weaponLength = startOffset = threshold = 0f;
        }
    }
    
    [Serializable]
    public struct AdsData
    {
        public CameraData cameraData;
        public AdsBlend adsTranslationBlend;
        public AdsBlend adsRotationBlend;
        public LocRot pointAimOffset;
        public float aimSpeed;
        public float changeSightSpeed;
        public float pointAimSpeed;

        public AdsData(float speed)
        {
            cameraData = null;
            pointAimOffset = LocRot.identity;
            aimSpeed = changeSightSpeed = pointAimSpeed = speed;
            adsTranslationBlend = adsRotationBlend = new AdsBlend();
        }
    }

    [Serializable]
    public struct ViewmodelOffset
    {
        public LocRot poseOffset;
        public LocRot rightHandOffset;
        public LocRot leftHandOffset;
    }
    
    [Serializable]
    public struct WeaponTransformData
    {
        public Transform pivotPoint;
        public Transform aimPoint;
        public Transform leftHandTarget;
    }
}