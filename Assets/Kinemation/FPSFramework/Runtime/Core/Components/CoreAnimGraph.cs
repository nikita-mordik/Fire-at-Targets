// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Types;

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kinemation.FPSFramework.Runtime.Core.Playables
{
    // Unity Animator sub-system
    [ExecuteInEditMode, Serializable]
    public class CoreAnimGraph : MonoBehaviour
    {
        [SerializeField] private AvatarMask upperBodyMask;
        [SerializeField] private RuntimeAnimatorController firstPersonAnimator;

        [Tooltip("Max blending poses")] private int maxPoseCount = 3;
        [Tooltip("Max blending clips")] private int maxAnimCount = 3;

        private Animator _animator;
        private PlayableGraph _playableGraph;
        
        private CoreAnimMixer _overlayPoseMixer;
        private CoreAnimMixer _slotMixer;
        private CoreAnimMixer _overrideMixer;

        private AnimationLayerMixerPlayable _dynamicAnimationMixer;
        private AnimationLayerMixerPlayable _masterMixer;
        
        private AnimatorControllerPlayable _firstPersonControllerPlayable;
        private AnimatorControllerPlayable _baseControllerPlayable;

        private float _poseProgress = 0f;

        private Quaternion outSpineRot = Quaternion.identity;
        private Quaternion targetSpineRot = Quaternion.identity;
        private Quaternion cacheSpineRot = Quaternion.identity;

        public bool InitPlayableGraph()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
            
            _animator = GetComponent<Animator>();
            _playableGraph = _animator.playableGraph;

            if (!_playableGraph.IsValid())
            {
                Debug.LogWarning(gameObject.name + " Animator Controller is not valid!");
                return false;
            }
            
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            
            _masterMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
            _dynamicAnimationMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
            
            _overlayPoseMixer = new CoreAnimMixer(_playableGraph, maxPoseCount, 0);
            _slotMixer = new CoreAnimMixer(_playableGraph, maxAnimCount, 1);
            _overrideMixer = new CoreAnimMixer(_playableGraph, maxAnimCount, 1);

            _baseControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph,
                _animator.runtimeAnimatorController);
            
            _slotMixer.mixer.ConnectInput(0, _overlayPoseMixer.mixer, 0, 1f);
            _overrideMixer.mixer.ConnectInput(0, _slotMixer.mixer, 0, 1f);
            _dynamicAnimationMixer.ConnectInput(0, _overrideMixer.mixer, 0, 1f);
            
            if (firstPersonAnimator != null)
            {
                _firstPersonControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, firstPersonAnimator);
                _dynamicAnimationMixer.ConnectInput(1, _firstPersonControllerPlayable, 0, 1f);
            }
            else
            {
                Debug.LogWarning("CoreAnimGraph: First Person Animator Controller is null!");
            }
            
            _masterMixer.ConnectInput(0, _baseControllerPlayable, 0, 1f);
            _masterMixer.ConnectInput(1, _dynamicAnimationMixer, 0, 1f);
            
            _masterMixer.SetLayerMaskFromAvatarMask(0, new AvatarMask());
            _masterMixer.SetLayerMaskFromAvatarMask(1, upperBodyMask);
            
            var output = AnimationPlayableOutput.Create(_playableGraph, "FPSAnimatorGraph", _animator);
            output.SetSourcePlayable(_masterMixer);

            _playableGraph.Play();
            return true;
        }

        public AnimatorControllerPlayable GetBaseAnimator()
        {
            return _baseControllerPlayable;
        }

        public AnimatorControllerPlayable GetFirstPersonAnimator()
        {
            return _firstPersonControllerPlayable;
        }
        
        public void UpdateGraph()
        {
            if (!Application.isPlaying) return;
            
            _overlayPoseMixer.Update();
            _slotMixer.Update();
            _overrideMixer.Update();
            
            _poseProgress = _overlayPoseMixer.BlendInWeight;

            outSpineRot = Quaternion.Slerp(cacheSpineRot, targetSpineRot, _slotMixer.BlendInWeight);
            outSpineRot = Quaternion.Slerp(outSpineRot, Quaternion.identity, _slotMixer.BlendOutWeight);
        }

        public Quaternion GetSpineOffset()
        {
            return outSpineRot;
        }
        
        public float GetCurveValue(string curveName)
        {
            return _slotMixer.GetCurveValue(curveName);
        }

        public float GetPoseProgress()
        {
            return _poseProgress;
        }

        public float GetGraphWeight()
        {
            if (Application.isPlaying) return !_masterMixer.IsValid() ? 0f : _masterMixer.GetInputWeight(1);
            return 0f;
        }

        public void SetGraphWeight(float weight)
        {
            if (!_playableGraph.IsValid() || !_masterMixer.IsValid())
            {
                return;
            }
            
            _masterMixer.SetInputWeight(1, Mathf.Clamp01(weight));
        }
        
        public void PlayPose(AnimSequence motion)
        {
            if (motion.clip == null)
            {
                return;
            }
            
            CoreAnimPlayable animPlayable = new CoreAnimPlayable(_playableGraph, motion.clip, null)
            {
                blendTime = motion.blendTime,
                autoBlendOut = false
            };

            animPlayable.playableClip.SetTime(0f);
            animPlayable.playableClip.SetSpeed(1f);
            _overlayPoseMixer.Play(animPlayable, upperBodyMask);
            
            SamplePose(motion.clip);
        }
        
        public void PlayAnimation(AnimSequence motion, float startTime)
        {
            if (motion.clip == null)
            {
                return;
            }

            cacheSpineRot = outSpineRot;
            targetSpineRot = motion.spineRotation;

            BlendTime blendTime = motion.blendTime;
            blendTime.startTime = startTime;

            CoreAnimPlayable animPlayable = new CoreAnimPlayable(_playableGraph, motion.clip, 
                motion.curves.ToArray())
            {
                blendTime = blendTime,
                autoBlendOut = true
            };

            animPlayable.playableClip.SetTime(startTime);
            animPlayable.playableClip.SetSpeed(blendTime.rateScale);

            _slotMixer.Play(animPlayable, motion.mask == null ? upperBodyMask : motion.mask,
                motion.isAdditive);
            
            CoreAnimPlayable overridePlayable = new CoreAnimPlayable(_playableGraph, motion.clip, null)
            {
                blendTime = blendTime,
                autoBlendOut = true
            };

            overridePlayable.playableClip.SetTime(startTime);
            overridePlayable.playableClip.SetSpeed(blendTime.rateScale);

            if (motion.overrideMask != null)
            {
                _overrideMixer.Play(overridePlayable, motion.overrideMask);
            }
        }

        public void StopAnimation(float blendTime)
        {
            _slotMixer.Stop(blendTime);
            _overrideMixer.Stop(blendTime);
        }

        public bool IsPlaying()
        {
            return _playableGraph.IsValid() && _playableGraph.IsPlaying();
        }
        
        // Samples overlay static pose, must be called during Update()
        public void SamplePose(AnimationClip clip)
        {
            clip.SampleAnimation(transform.gameObject, 0f);
        }

        public AvatarMask GetUpperBodyMask()
        {
            return upperBodyMask;
        }
        
        private void OnDestroy()
        {
            if (!_playableGraph.IsValid())
            {
                return;
            }

            _playableGraph.Stop();
            _playableGraph.Destroy();
        }

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private AnimationClip previewClip;
        [SerializeField] [HideInInspector] private bool loopPreview;
        
        public bool InitPlayableEditorGraph()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogWarning("FPSAnimator Preview: Animator component not found!");
                return false;
            }

            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }

            if (_masterMixer.IsValid())
            {
                _masterMixer.Destroy();
            }
            
            _playableGraph = PlayableGraph.Create();
            _masterMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 1);
            
            var output = AnimationPlayableOutput.Create(_playableGraph, "FPSAnimatorEditorGraph", _animator);
            output.SetSourcePlayable(_masterMixer);
            
            return true;
        }
        
        private void LoopPreview()
        {
            if (!_playableGraph.IsPlaying())
            {
                EditorApplication.update -= LoopPreview;
            }
            
            if (loopPreview && _playableGraph.IsValid() 
                            && _masterMixer.GetInput(0).GetTime() >= previewClip.length)
            {
                _masterMixer.GetInput(0).SetTime(0f);
            }
        }
        
        public void StartPreview()
        {
            if (!InitPlayableEditorGraph())
            {
                return;
            }

            if (previewClip != null)
            {
                var previewPlayable = AnimationClipPlayable.Create(_playableGraph, previewClip);
                previewPlayable.SetTime(0f);
                previewPlayable.SetSpeed(1f);

                if (_masterMixer.GetInput(0).IsValid())
                {
                    _masterMixer.DisconnectInput(0);
                }

                _masterMixer.ConnectInput(0, previewPlayable, 0, 1f);
                EditorApplication.update += LoopPreview;
            }
            else
            {
                var controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph,
                    _animator.runtimeAnimatorController);
                
                _masterMixer.ConnectInput(0, controllerPlayable, 0, 1f);
            }

            _playableGraph.Play();
        }

        public void StopPreview()
        {
            if (!_playableGraph.IsValid()) return;
            
            _masterMixer.DisconnectInput(0);
            _playableGraph.Stop();
            _playableGraph.Destroy();
            
            EditorApplication.update -= LoopPreview;
        }
#endif
    }
}