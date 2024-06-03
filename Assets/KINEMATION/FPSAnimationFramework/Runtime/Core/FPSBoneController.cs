// Designed by KINEMATION, 2024.

using System;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    public struct CachedPose
    {
        public KPose[] poses;
        public float blendTime;
        public float blendAmount;
        public float blendPlayback;
        public EaseMode easeMode;
    }
    
    public class FPSBoneController : MonoBehaviour
    {
        protected KRigComponent _rigComponent;
        protected FPSAnimatorProfile _activeProfile;
        protected List<FPSAnimatorLayerState> _layerStates;
        
        // Cached pose from the previous profile.
        protected List<CachedPose> _cachedPoses;
        // What bones will be affected by blending out.
        protected HashSet<int> _blendBonesIndexes;
        // Character original pose before applying IK. Only used when blending.
        protected Dictionary<int, KTransform> _sourcePose;

        protected bool IsBlendingOut()
        {
            return _cachedPoses.Count > 0;
        }

        public void Initialize()
        {
            _rigComponent = GetComponentInChildren<KRigComponent>();
            _rigComponent.Initialize();
            
            _layerStates = new List<FPSAnimatorLayerState>();
            
            _cachedPoses = new List<CachedPose>();
            _sourcePose = new Dictionary<int, KTransform>();

            _blendBonesIndexes = new HashSet<int>();

            if (_rigComponent == null)
            {
                Debug.LogError("FPSAnimatorBoneController: no RigComponent found!");
            }
        }

        public virtual void GameThreadUpdate()
        {
            foreach (var state in _layerStates)
            {
                state.OnGameThreadUpdate();
            }
        }

        public virtual void CachePose()
        {
            if (!IsBlendingOut()) return;

            _sourcePose.Clear();

            // Update the source pose by getting the transforms directly from the hierarchy.
            foreach (var index in _blendBonesIndexes)
            {
                Transform reference = _rigComponent.GetRigTransform(index);
                _sourcePose.Add(index, new KTransform(reference));
            }
        }

        public virtual void EvaluatePose()
        {
            _rigComponent.AnimateVirtualElements();

            foreach (var state in _layerStates)
            {
                state.OnPreEvaluatePose();
            }
            
            // Apply the active animator states.
            foreach (var state in _layerStates)
            {
                state.UpdateStateWeight();
                state.OnEvaluatePose();
            }
        }

        public virtual void PostEvaluatePose()
        {
            // Apply the active animator states.
            foreach (var state in _layerStates)
            {
                state.OnPostEvaluatedPose();
            }
        }

        public virtual void ApplyCachedPose()
        {
            // ReSharper Disable All
            if (!IsBlendingOut()) return;

            int count = _cachedPoses.Count;
            List<int> posesToDispose = new List<int>();

            for (int i = 0; i < count; i++)
            {
                CachedPose cachedPose = _cachedPoses[i];
                float weight = cachedPose.blendAmount;
                
                if (Mathf.Approximately(weight, 1f))
                {
                    posesToDispose.Add(i);
                    continue;
                }

                foreach (var pose in cachedPose.poses)
                {
                    Transform boneReference = _rigComponent.GetRigTransform(pose.element);
                    
                    KTransform activePose = new KTransform(boneReference);
                    KTransform sourcePose = _sourcePose[pose.element.index];

                    // Apply the cached pose first.
                    boneReference.position = sourcePose.position;
                    boneReference.rotation = sourcePose.rotation;
                    
                    // Apply the cached pose.
                    KAnimationMath.ModifyTransform(transform, boneReference, pose);
                    boneReference.position = activePose.position;

                    // Interpolate.
                    boneReference.rotation = Quaternion.Slerp(boneReference.rotation, activePose.rotation, 
                        cachedPose.blendAmount);
                }

                cachedPose.blendPlayback = Mathf.Clamp(cachedPose.blendPlayback + Time.deltaTime, 0f, 
                    cachedPose.blendTime);
                
                float normalizedTime = Mathf.Clamp01(cachedPose.blendPlayback / cachedPose.blendTime);
                cachedPose.blendAmount = KCurves.Ease(0f, 1f, normalizedTime, cachedPose.easeMode);

                _cachedPoses[i] = cachedPose;
            }

            // Remove the blended out poses.
            foreach (var index in posesToDispose)
            {
                _cachedPoses.RemoveAt(index);
            }

            if (_cachedPoses.Count == 0)
            {
                _blendBonesIndexes.Clear();
            }
        }

        public void UnlinkAnimatorProfile()
        {
            int count = _activeProfile.settings.Count;
            List<KPose> bonePoses = new List<KPose>();
            
            for (int i = 0; i < count; i++)
            {
                var activeSetting = _activeProfile.settings[i];
                
                _layerStates[i].RegisterBones(ref _blendBonesIndexes);
                _layerStates[i].CachePoses(ref bonePoses);
                _layerStates[i].OnDestroyed();
            }
            
            _cachedPoses.Add(new CachedPose()
            {
                blendAmount = 0f,
                blendPlayback = 0f,
                blendTime = _activeProfile.blendOutTime,
                poses = bonePoses.ToArray(),
                easeMode = _activeProfile.easeMode
            });
            
            _layerStates.Clear();
            _activeProfile = null;
        }

        public void LinkAnimatorProfile(FPSAnimatorProfile newProfile)
        {
            if (newProfile == null)
            {
                Debug.LogWarning($"FPSBoneController: Profile is null, use UnlinkAnimatorProfile instead");
                return;
            }

            if (newProfile.Equals(_activeProfile)) return;
            
            // Maps the setting to the target state.
            Dictionary<Type, FPSAnimatorLayerSettings> linkedSettingsMap 
                = new Dictionary<Type, FPSAnimatorLayerSettings>();
            
            // Maps the setting type to the state.
            Dictionary<Type, FPSAnimatorLayerState> linkedStatesMap 
                = new Dictionary<Type, FPSAnimatorLayerState>();

            // Get a map of types and settings from the new profile.
            foreach (var layerSettings in newProfile.settings)
            {
                linkedSettingsMap.TryAdd(layerSettings.GetType(), layerSettings);
            }
            
            if (_activeProfile == null || !KAnimationMath.IsWeightRelevant(newProfile.blendInTime))
            {
                Dispose();
            }
            else
            {
                int count = _activeProfile.settings.Count;
                List<KPose> bonePoses = new List<KPose>();
                
                for (int i = 0; i < count; i++)
                {
                    var activeSetting = _activeProfile.settings[i];

                    if (linkedSettingsMap.TryGetValue(activeSetting.GetType(), out var newSetting)
                        && newSetting.linkDynamically)
                    {
                        // If types are the same, map the type to the desired state.
                        linkedStatesMap.TryAdd(activeSetting.GetType(), _layerStates[i]);
                        continue;
                    }
                    
                    // No collision - Cache and Destroy the layer.
                    _layerStates[i].RegisterBones(ref _blendBonesIndexes);
                    _layerStates[i].CachePoses(ref bonePoses);
                    _layerStates[i].OnDestroyed();
                }
                
                // Finally add the Cached Pose to the pending list.
                _cachedPoses.Add(new CachedPose()
                {
                    blendAmount = 0f,
                    blendPlayback = 0f,
                    blendTime = newProfile.blendInTime,
                    poses = bonePoses.ToArray(),
                    easeMode = newProfile.easeMode
                });
            }
            
            _layerStates.Clear();
            _activeProfile = newProfile;
            
            foreach (var setting in _activeProfile.settings)
            {
                // If the setting type is in the map, we must dynamically link it.
                if (linkedStatesMap.TryGetValue(setting.GetType(), out var layerState))
                {
                    layerState.OnLayerLinked(setting);
                    _layerStates.Add(layerState);
                    continue;
                }
                
                var state = setting.CreateState();
                state.InitializeComponents(gameObject, setting);
                state.InitializeState(setting);
                state.RegisterBones(ref _blendBonesIndexes);
                _layerStates.Add(state);
            }
        }

        // Will find and update settings for the current active animator state.
        public void LinkAnimatorLayer(FPSAnimatorLayerSettings newSettings)
        {
            int count = _layerStates.Count;

            for (int i = 0; i < count; i++)
            {
                if (_activeProfile.settings[i].GetType() == newSettings.GetType())
                {
                    _layerStates[i].OnLayerLinked(newSettings);
                }
            }
        }

        public void UpdateEntity(FPSAnimatorEntity newEntity)
        {
            foreach (var state in _layerStates) state.OnEntityUpdated(newEntity);
        }

        public void Dispose()
        {
            foreach (var state in _layerStates) state.OnDestroyed();
            _layerStates.Clear();
        }
    }
}