// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.KAnimationCore.Runtime.Input;
using KINEMATION.KAnimationCore.Runtime.Rig;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    public abstract class FPSAnimatorLayerState
    {
        protected float Weight { get; private set; }
        
        protected KRigComponent _rigComponent;
        protected UserInputController _inputController;
        protected IPlayablesController _playablesController;
        protected GameObject _owner;

        // Only used to update the layer weight.
        private FPSAnimatorLayerSettings _internalSettings;
        
        protected float GetCurveBlendValue(CurveBlend blend)
        {
            float value = 0f;
            
            if (blend.source == ECurveSource.Input)
            {
                value = _inputController.GetValue<float>(blend.name);
            }
            else
            {
                value = _playablesController.GetCurveValue(blend.name, blend.source == ECurveSource.Animator);
            }

            value = Mathf.Clamp01(value);
            return blend.mode == ECurveBlendMode.Direct ? value : 1f - value;
        }

        public void UpdateStateWeight()
        {
            Weight = GetWeight(_internalSettings);
        }

        // Returns the weight/alpha for this state based on the blending profile.
        private float GetWeight(FPSAnimatorLayerSettings settings)
        {
            // Enable by default.
            float weight = 1f;

            // Invalid settings.
            if (settings == null)
            {
                return 0f;
            }

            // Scale the weight based on the curve values.
            foreach (var blend in settings.curveBlending)
            {
                float value = GetCurveBlendValue(blend);
                value = Mathf.Lerp(blend.clampMin, 1f, value);
                weight *= value;
            }

            // Finally scale the result by the global settings alpha.
            return Mathf.Clamp01(weight * settings.alpha);
        }
        
        // Called on Start.
        public void InitializeComponents(GameObject owner, FPSAnimatorLayerSettings settings)
        {
            _owner = owner;
            _rigComponent = _owner.GetComponentInChildren<KRigComponent>();
            _inputController = _owner.GetComponentInChildren<UserInputController>();
            _playablesController = _owner.GetComponent<IPlayablesController>();
            _internalSettings = settings;
        }
        
        public virtual void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
        }

        // Registers bones, modified by the animation layer.
        public virtual void RegisterBones(ref HashSet<int> registeredBones)
        {
        }

        // Called on Update.
        public virtual void OnGameThreadUpdate()
        {
        }

        // Called on LateUpdate before any modifications.
        public virtual void OnPreEvaluatePose()
        {
        }

        // Called on LateUpdate. Use it only to apply the actual pose.
        public virtual void OnEvaluatePose()
        {
        }

        public virtual void OnPostEvaluatedPose()
        {
            
        }

        public virtual void OnDestroyed()
        {
        }

        // Called when settings for this state are updated.
        // Use this to initiate a blending from previous settings to the new ones.
        public virtual void OnLayerLinked(FPSAnimatorLayerSettings newSettings)
        {
        }

        public virtual void OnEntityUpdated(FPSAnimatorEntity newEntity)
        {
        }

        // Called when the layer is supposed to be disabled.
        public virtual void CachePoses(ref List<KPose> cachedPoses)
        {
        }
    }
}