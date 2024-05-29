// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.WeaponLayer
{
    public abstract class WeaponLayerSettings : FPSAnimatorLayerSettings
    {
        [Header("Weapon Layer General")]
        public KRigElement weaponIkBone = new KRigElement(-1, FPSANames.IkWeaponBone);
        public KRigElement rightHandElbow = new KRigElement(-1, FPSANames.IkRightElbow);
        public KRigElement leftHandElbow = new KRigElement(-1, FPSANames.IkLeftElbow);
        
        public Vector3 animatedPivotOffset = Vector3.zero;

        [Tooltip("How much we want to affect the elbows: 1 - fully affected, 0 - no effect.")]
        [Range(0f, 1f)] public float hintTargetWeight = 1f;

#if UNITY_EDITOR
        public override void OnRigUpdated()
        {
            UpdateRigElement(ref weaponIkBone);
            UpdateRigElement(ref rightHandElbow);
            UpdateRigElement(ref leftHandElbow);
        }
#endif
    }
}