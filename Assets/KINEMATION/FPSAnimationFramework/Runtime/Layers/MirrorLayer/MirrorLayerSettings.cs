// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.MirrorLayer
{
    public class MirrorLayerSettings : FPSAnimatorLayerSettings
    {
        public List<KRigElement> bonesToMirror = new List<KRigElement>();
        public Vector3 mirrorAxis = Vector3.forward;

        public bool mirrorRotation = true;
        public bool mirrorTranslation = true;
        
        public override FPSAnimatorLayerState CreateState()
        {
            return new MirrorLayerState();
        }
        
#if UNITY_EDITOR
        public override void OnRigUpdated()
        {
            for (int i = 0; i < bonesToMirror.Count; i++)
            {
                var element = bonesToMirror[i];
                UpdateRigElement(ref element);
                bonesToMirror[i] = element;
            }
        }
#endif
    }
}