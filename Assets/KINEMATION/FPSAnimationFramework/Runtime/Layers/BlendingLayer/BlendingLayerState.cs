// Designed by KINEMATION, 2024.

using System;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.BlendingLayer
{
    public class BlendingLayerState : FPSAnimatorLayerState
    {
        private BlendingLayerSettings _settings;
        private List<KTransform> _cachedPose;
        private List<Transform> _elementTransforms;
        private List<KTransform> _basePose;

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (BlendingLayerSettings) newSettings;

            if (_settings.desiredPose == null) return;
            
            _rigComponent.CacheHierarchyPose();
            
            _settings.desiredPose.SampleAnimation(_owner, 0f);

            _basePose = new List<KTransform>();
            _cachedPose = new List<KTransform>();
            _elementTransforms = new List<Transform>();

            foreach (var element in _settings.blendingElements)
            {
                Transform transform = _rigComponent.GetRigTransform(element.elementToBlend);
                _elementTransforms.Add(transform);
                _cachedPose.Add(new KTransform(_owner.transform).GetRelativeTransform(new KTransform(transform),
                    false));
            }
            
            _rigComponent.ApplyHierarchyCachedPose();
        }

        public override void OnEvaluatePose()
        {
            // Cache the poses before any modification.
            // This is required to not affect the child bones.
            
            int count = _elementTransforms.Count;
            for (int i = 0; i < count; i++) _basePose.Add(new KTransform(_elementTransforms[i]));

            for (int i = 0; i < count; i++)
            {
                Transform transform = _elementTransforms[i];
                KTransform source = _basePose[i];
                KTransform target = new KTransform(_owner.transform).GetWorldTransform(_cachedPose[i], false);
                float weight = Weight * _settings.blendingElements[i].weight;
                
                if(_settings.blendPosition) transform.position = Vector3.Lerp(source.position, target.position, weight);
                transform.rotation = Quaternion.Slerp(source.rotation, target.rotation, weight);
            }
            
            _basePose.Clear();
        }
    }
}