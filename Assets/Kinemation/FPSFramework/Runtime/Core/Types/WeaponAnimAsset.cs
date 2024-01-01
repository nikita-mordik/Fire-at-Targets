// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Recoil;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [System.Serializable, CreateAssetMenu(fileName = "NewWeaponAsset", menuName = "FPS Animator/WeaponAnimAsset")]
    public class WeaponAnimAsset : ScriptableObject
    {
        [Header("General"), Tooltip("Adjusts weapon model rotation")]
        public Quaternion rotationOffset = Quaternion.identity;
        public AimOffsetTable aimOffsetTable;
        public RecoilAnimData recoilData;
        public AnimSequence overlayPose;
        
        [Tooltip("Defines weapon default position and rotation pose.")]
        public LocRot weaponBone = LocRot.identity;
        
        [Header("AdsLayer")]
        public AdsData adsData;
        
        [Tooltip("Offsets the arms pose")]
        public ViewmodelOffset viewmodelOffset = new ViewmodelOffset()
        {
            poseOffset = LocRot.identity,
            rightHandOffset = LocRot.identity,
            leftHandOffset = LocRot.identity
        };
        
        public LocRot viewOffset = LocRot.identity;
        
        [FormerlySerializedAs("springData")] [Header("SwayLayer")]
        public LocRotSpringData aimSwaySettings;
        [FormerlySerializedAs("freeAimData")] public FreeAimData freeAimSettings;
        [FormerlySerializedAs("moveSwayData")] public MoveSwayData moveSwaySettings;
        
        [Header("WeaponCollision")] 
        public GunBlockData blockData;

        [Header("Pivoting")] 
        public Vector3 adsRecoilOffset;
        public Vector3 adsSwayOffset;
    }
}