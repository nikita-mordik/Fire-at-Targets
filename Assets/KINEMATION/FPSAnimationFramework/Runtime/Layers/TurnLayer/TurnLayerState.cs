// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.TurnLayer
{
    public class TurnLayerState : FPSAnimatorLayerState
    {
        private TurnLayerSettings _settings;
        
        private Transform _characterRootBone;
        private int _turnInputProperty;
        private int _mouseDeltaProperty;

        private float _playBack = 1f;
        private float _turnAngle = 0f;
        private float _cachedTurnAngle;
        
        private bool _isTurning;
        private Animator _animator;

        private int _turnRightHash;
        private int _turnLeftHash;

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (TurnLayerSettings) newSettings;
            if (_settings == null) return;

            _turnInputProperty = _inputController.GetPropertyIndex(_settings.turnInputProperty);
            _mouseDeltaProperty = _inputController.GetPropertyIndex(_settings.mouseDeltaInputProperty);
            _characterRootBone = _rigComponent.GetRigTransform(_settings.characterRootBone);

            _animator = _owner.GetComponentInChildren<Animator>();
            _turnRightHash = Animator.StringToHash(_settings.animatorTurnRightTrigger);
            _turnLeftHash = Animator.StringToHash(_settings.animatorTurnLeftTrigger);
        }

        public override void OnLayerLinked(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (TurnLayerSettings) newSettings;
        }

        public override void OnPreEvaluatePose()
        {
            float mouseDelta = _inputController.GetValue<Vector4>(_mouseDeltaProperty).x;
            _turnAngle -= mouseDelta;

            _turnAngle *= Weight;

            if (!_isTurning && Mathf.Abs(_turnAngle) > _settings.angleThreshold)
            {
                _cachedTurnAngle = _turnAngle;
                _isTurning = true;
                _playBack = 0f;
                
                _animator.SetTrigger(_turnAngle < 0f ? _turnRightHash : _turnLeftHash);
            }
            
            if (_isTurning)
            {
                _playBack = Mathf.Clamp01(_playBack + Time.deltaTime * _settings.turnSpeed);
                float alpha = _settings.turnCurve.Evaluate(_playBack);

                _turnAngle = Mathf.Lerp(_cachedTurnAngle, 0f, alpha);

                if (Mathf.Approximately(_playBack, 1f)) _isTurning = false;
            }
            
            _inputController.SetValue(_turnInputProperty, -_turnAngle);
        }

        public override void OnEvaluatePose()
        {
            Quaternion offset = Quaternion.Euler(0f, _turnAngle, 0f);
            KAnimationMath.RotateInSpace(_owner.transform, _characterRootBone, offset, 1f);
        }
    }
}
