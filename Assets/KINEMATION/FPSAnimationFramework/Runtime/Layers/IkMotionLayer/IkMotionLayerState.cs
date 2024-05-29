// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.IkMotionLayer
{
    public class IkMotionLayerState : FPSAnimatorLayerState
    {
        private IkMotionLayerSettings _settings;

        private Transform _boneToAnimate;
        
        private KTransform _result = KTransform.Identity;
        private KTransform _cachedResult = KTransform.Identity;
        
        private float _playback = 0f;
        private float _length = 0f;
        private bool _isPlaying;
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (IkMotionLayerSettings) newSettings;
            // Retrieve the bone reference.
            _boneToAnimate = _rigComponent.GetRigTransform(_settings.boneToAnimate);
        }

        public override void OnEvaluatePose()
        {
            if (!_isPlaying) return;
            
            // Update the playback.
            _playback = Mathf.Clamp(_playback + Time.deltaTime * _settings.playRate, 0f, _length);

            // Get the curve values.
            Vector3 euler = _settings.rotationCurves.GetValue(_playback);
            euler.x *= _settings.rotationScale.x;
            euler.y *= _settings.rotationScale.y;
            euler.z *= _settings.rotationScale.z;
            
            Quaternion rotation = Quaternion.Euler(euler);
            Vector3 position = _settings.translationCurves.GetValue(_playback);

            position.x *= _settings.translationScale.x;
            position.y *= _settings.translationScale.y;
            position.z *= _settings.translationScale.z;

            // Compute the blending factor.
            float blendAlpha = 1f;
            if (!Mathf.Approximately(_settings.blendTime, 0f))
            {
                blendAlpha = Mathf.Clamp01(_playback / _settings.blendTime);
            }

            // Blend between the cache and current value.
            rotation = Quaternion.Slerp(_cachedResult.rotation, rotation, blendAlpha);
            position = Vector3.Lerp(_cachedResult.position, position, blendAlpha);

            _result.rotation = rotation;
            _result.position = position;

            // Finally apply the animation.
            KAnimationMath.MoveInSpace(_owner.transform, _boneToAnimate, position, Weight);
            KAnimationMath.RotateInSpace(_owner.transform, _boneToAnimate, rotation, Weight);
            
            // Disable tick if reached the end of the track.
            if (Mathf.Approximately(_playback, 1f))
            {
                _isPlaying = false;
            }
        }

        public override void OnLayerLinked(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (IkMotionLayerSettings) newSettings;
            
            _isPlaying = true;
            _cachedResult = _result;
            _playback = 0f;

            _length = Mathf.Max(_settings.rotationCurves.GetCurveLength(),
                _settings.translationCurves.GetCurveLength());
        }
    }
}