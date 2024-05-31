// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Attributes;
using KINEMATION.KAnimationCore.Runtime.Input;

using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace KINEMATION.FPSAnimationFramework.Runtime.Playables
{
    [HelpURL("https://kinemation.gitbook.io/scriptable-animation-system/workflow/components")]
    public class FPSPlayablesController : MonoBehaviour, IPlayablesController
    {
        [Header("General Settings")] 
        [SerializeField] protected AvatarMask upperBodyMask;

        [SerializeField] [InputProperty]
        protected string playablesWeightProperty = FPSANames.PlayablesWeight;
        
        [Header("Editor Animation Preview")]
        [SerializeField] protected AnimationClip animationToPreview;
        [SerializeField] protected AnimationClip defaultPose;
        [SerializeField] protected bool loopPreview;
        
        protected int _maxPoseCount = 3;
        protected int _maxAnimCount = 3;
        
        protected Animator _animator;
        
        protected PlayableGraph _playableGraph;
        protected FPSAnimatorMixer _overlayPoseMixer;
        protected FPSAnimatorMixer _slotMixer;
        protected FPSAnimatorMixer _overrideMixer;

        protected AnimationLayerMixerPlayable _dynamicAnimationMixer;
        protected AnimationLayerMixerPlayable _masterMixer;
        
        protected AnimatorControllerPlayable _controllerPlayable;
        
        protected UserInputController _inputController;
        protected float _controllerWeight = 1f;

        protected int _playablesWeightPropertyIndex;

        protected virtual void Update()
        {
            if (!Application.isPlaying) return;
            
            _overlayPoseMixer.Update();
            _slotMixer.Update();
            _overrideMixer.Update();

            float weight = _controllerWeight;
            if (_inputController != null)
            {
                weight *= Mathf.Clamp01(_inputController.GetValue<float>(_playablesWeightPropertyIndex));
            }
            
            _masterMixer.SetInputWeight(1, Mathf.Clamp01(weight));
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
        
        public virtual bool InitializeController()
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
            
            _overlayPoseMixer = new FPSAnimatorMixer(_playableGraph, _maxPoseCount, 0);
            _slotMixer = new FPSAnimatorMixer(_playableGraph, _maxAnimCount, 1);
            _overrideMixer = new FPSAnimatorMixer(_playableGraph, _maxAnimCount, 1);

            _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph,
                _animator.runtimeAnimatorController);
            
            _slotMixer.mixer.ConnectInput(0, _overlayPoseMixer.mixer, 0, 1f);
            _overrideMixer.mixer.ConnectInput(0, _slotMixer.mixer, 0, 1f);
            _dynamicAnimationMixer.ConnectInput(0, _overrideMixer.mixer, 0, 1f);
            
            _masterMixer.ConnectInput(0, _controllerPlayable, 0, 1f);
            _masterMixer.ConnectInput(1, _dynamicAnimationMixer, 0, 1f);
            
            _masterMixer.SetLayerMaskFromAvatarMask(0, new AvatarMask());
            _masterMixer.SetLayerMaskFromAvatarMask(1, upperBodyMask);
            
            var output = AnimationPlayableOutput.Create(_playableGraph, "FPSAnimatorGraph", _animator);
            output.SetSourcePlayable(_masterMixer);

            _playableGraph.Play();
            _inputController = GetComponent<UserInputController>();

            if (_inputController == null) return true;
            _playablesWeightPropertyIndex = _inputController.GetPropertyIndex(playablesWeightProperty);
            
            return true;
        }
        
        public void SetControllerWeight(float weight)
        {
            if (!_playableGraph.IsValid() || !_masterMixer.IsValid())
            {
                return;
            }

            _controllerWeight = Mathf.Clamp01(weight);
        }

        public virtual bool PlayPose(FPSAnimationAsset asset)
        {
            if (asset.clip == null)
            {
                return false;
            }
            
            FPSAnimatorPlayable animPlayable = new FPSAnimatorPlayable(_playableGraph, asset.clip, null)
            {
                blendTime = asset.blendTime,
                autoBlendOut = false
            };

            animPlayable.playable.SetTime(0f);
            animPlayable.playable.SetSpeed(1f);
            _overlayPoseMixer.Play(animPlayable, upperBodyMask);

            return true;
        }

        public virtual bool PlayAnimation(FPSAnimationAsset asset, float startTime = 0f)
        {
            if (asset.clip == null)
            {
                return false;
            }

            BlendTime blendTime = asset.blendTime;
            blendTime.startTime = startTime;

            FPSAnimatorPlayable animPlayable = new FPSAnimatorPlayable(_playableGraph, asset.clip, 
                asset.curves.ToArray())
            {
                blendTime = blendTime,
                autoBlendOut = true
            };

            animPlayable.playable.SetTime(startTime);
            animPlayable.playable.SetSpeed(blendTime.rateScale);

            _slotMixer.Play(animPlayable, asset.mask == null ? upperBodyMask : asset.mask, asset.isAdditive);
            
            FPSAnimatorPlayable overridePlayable = new FPSAnimatorPlayable(_playableGraph, asset.clip, null)
            {
                blendTime = blendTime,
                autoBlendOut = true
            };

            overridePlayable.playable.SetTime(startTime);
            overridePlayable.playable.SetSpeed(blendTime.rateScale);

            if (asset.overrideMask != null)
            {
                _overrideMixer.Play(overridePlayable, asset.overrideMask);
            }

            return true;
        }

        public virtual void StopAnimation(float blendOutTime)
        {
            _slotMixer.Stop(blendOutTime);
            _overrideMixer.Stop(blendOutTime);
        }

        public bool IsPlaying()
        {
            return _playableGraph.IsValid() && _playableGraph.IsPlaying();
        }

        public virtual float GetCurveValue(string curveName, bool isAnimator = false)
        {
            return isAnimator ? _animator.GetFloat(curveName) : _slotMixer.GetCurveValue(curveName);
        }
        
#if UNITY_EDITOR
        public virtual bool InitializeControllerEditor()
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

        public virtual void StartEditorPreview()
        {
            if (!InitializeControllerEditor())
            {
                return;
            }

            if (animationToPreview != null)
            {
                var previewPlayable = AnimationClipPlayable.Create(_playableGraph, animationToPreview);
                previewPlayable.SetTime(0f);
                previewPlayable.SetSpeed(1f);

                if (_masterMixer.GetInput(0).IsValid())
                {
                    _masterMixer.DisconnectInput(0);
                }

                _masterMixer.ConnectInput(0, previewPlayable, 0, 1f);
                EditorApplication.update += LoopEditorPreview;
            }
            else
            {
                var controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph,
                    _animator.runtimeAnimatorController);
                
                _masterMixer.ConnectInput(0, controllerPlayable, 0, 1f);
            }

            _playableGraph.Play();
            
            EditorApplication.QueuePlayerLoopUpdate();
        }

        public virtual void LoopEditorPreview()
        {
            if (!_playableGraph.IsPlaying())
            {
                EditorApplication.update -= LoopEditorPreview;
            }
            
            if (loopPreview && _playableGraph.IsValid() 
                            && _masterMixer.GetInput(0).GetTime() >= animationToPreview.length)
            {
                _masterMixer.GetInput(0).SetTime(0f);
            }
        }

        public virtual void StopEditorPreview()
        {
            if (!_playableGraph.IsValid()) return;
            
            _masterMixer.DisconnectInput(0);
            _playableGraph.Stop();
            _playableGraph.Destroy();
            
            EditorApplication.update -= LoopEditorPreview;

            if (defaultPose != null)
            {
                defaultPose.SampleAnimation(gameObject, 0f);
            }
        }
#endif
    }
}