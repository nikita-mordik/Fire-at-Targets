// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Attributes;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Recoil
{
    [CreateAssetMenu(fileName = "NewRecoilAnimData", menuName = FPSANames.FileMenuGeneral + "Recoil Data")]
    public class RecoilAnimData : ScriptableObject
    {
        [Tab("Recoil Targets")]
        
        [Header("Rotation Targets")]
        public Vector2 pitch;
        public Vector4 roll = new Vector4(0f, 0f, 0f, 0f);
        public Vector4 yaw = new Vector4(0f, 0f, 0f, 0f);

        [Header("Translation Targets")] 
        public Vector2 kickback;
        public Vector2 kickUp;
        public Vector2 kickRight;
    
        [Header("Aiming Multipliers")]
        public Vector3 aimRot;
        public Vector3 aimLoc;
        
        [Tab("Smoothing")]
    
        [Header("Auto/Burst Settings")]
        public Vector3 smoothRot;
        public Vector3 smoothLoc;
        
        public Vector3 extraRot;
        public Vector3 extraLoc;
        
        [Tab("Layers")]
    
        [Header("Noise Layer")]
        public Vector2 noiseX;
        public Vector2 noiseY;

        public Vector2 noiseAccel;
        public Vector2 noiseDamp;
    
        public float noiseScalar = 1f;
    
        [Header("Pushback Layer")]
        public float pushAmount = 0f;
        public float pushAccel;
        public float pushDamp;
        
        [Tab("Misc")]
        
        public Vector3 hipPivotOffset;
        public Vector3 aimPivotOffset;
        public bool smoothRoll;
        public float playRate;
    
        [Header("Recoil Curves")]
        public RecoilCurves recoilCurves = new RecoilCurves(
            new[] { new Keyframe(0f, 0f), new Keyframe(1f, 0f) });
    }
}
