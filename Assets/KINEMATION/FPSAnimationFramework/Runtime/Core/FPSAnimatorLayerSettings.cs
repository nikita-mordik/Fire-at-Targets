// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Attributes;
using KINEMATION.KAnimationCore.Runtime.Attributes;
using KINEMATION.KAnimationCore.Runtime.Rig;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    public enum ECurveBlendMode
    {
        Direct,
        Mask
    }

    public enum ECurveSource
    {
        Animator,
        Playables,
        Input
    }
    
    [Serializable]
    public struct CurveBlend
    {
        public string name;
        public ECurveBlendMode mode;
        [Range(0f, 1f)] public float clampMin;
        [HideInInspector] public ECurveSource source;
    }
    
    [HelpURL("https://kinemation.gitbook.io/scriptable-animation-system/fundamentals/animator-layer")]
    public abstract class FPSAnimatorLayerSettings : ScriptableObject, IRigUser, IRigObserver
    {
        [ShowStandalone] public KRig rigAsset;
        [Range(0f, 1f)] public float alpha = 1f;
        [CurveSelector] public List<CurveBlend> curveBlending = new List<CurveBlend>();
        
        [Tooltip("Will call OnUpdateSettings on the layer state if true.")]
        public bool linkDynamically;
        
        public virtual FPSAnimatorLayerState CreateState() { return null; }
        
        public KRig GetRigAsset() { return rigAsset; }
        
        public virtual void OnRigUpdated()
        {
            // Update bone indices here.
        }
        
#if UNITY_EDITOR
        [HideInInspector] public bool isStandalone = true;
        private KRig _cachedRigAsset;
        
        protected void UpdateRigElement(ref KRigElement element)
        {
            element = rigAsset.GetElementByName(element.name);
        }

        protected void OnValidate()
        {
            if (!isStandalone) return;
            
            if (rigAsset == _cachedRigAsset)
            {
                return;
            }

            if (_cachedRigAsset != null)
            {
                _cachedRigAsset.UnRegisterObserver(this);
            }

            if (rigAsset != null)
            {
                rigAsset.RegisterRigObserver(this);
                OnRigUpdated();
            }

            _cachedRigAsset = rigAsset;
        }
#endif
    }
}