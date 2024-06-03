// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.LookLayer
{
    public class LookLayerState : FPSAnimatorLayerState
    {
        private LookLayerSettings _settings;
        private Vector4 _lookInput;

        private int _mouseInputPropertyIndex;
        private int _leanPropertyIndex;
        private int _turnProperty;

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (LookLayerSettings) newSettings;

            _mouseInputPropertyIndex = _inputController.GetPropertyIndex(_settings.mouseInputProperty);
            _leanPropertyIndex = _inputController.GetPropertyIndex(_settings.leanInputProperty);
            _turnProperty = _inputController.GetPropertyIndex(_settings.turnOffsetProperty);
        }

        public override void OnLayerLinked(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (LookLayerSettings) newSettings;
        }

        public override void OnEvaluatePose()
        {
            float turnInput = _inputController.GetValue<float>(_turnProperty);
            _lookInput = _inputController.GetValue<Vector4>(_mouseInputPropertyIndex);
            
            if (_settings.useTurnOffset)
            {
                _lookInput.x = turnInput;
            }
            
            float lean = _inputController.GetValue<float>(_leanPropertyIndex);
            
            float fraction = lean / 90f;
            bool sign = fraction > 0f;
            
            foreach (var element in _settings.rollOffsetElements)
            {
                float angle = sign ? element.clampedAngle.x : element.clampedAngle.y;
                KAnimationMath.RotateInSpace(_owner.transform, _rigComponent.GetRigTransform(element.rigElement), 
                    Quaternion.Euler(0f, 0f, angle * fraction), Weight);
            }
            
            fraction = _lookInput.x / 90f;
            sign = fraction > 0f;
            
            foreach (var element in _settings.yawOffsetElements)
            {
                float angle = sign ? element.clampedAngle.x : element.clampedAngle.y;
                KAnimationMath.RotateInSpace(_owner.transform, _rigComponent.GetRigTransform(element.rigElement), 
                    Quaternion.Euler(0f, angle * fraction, 0f), Weight);
            }
            
            fraction = _lookInput.y / 90f;
            sign = fraction > 0f;
            
            Quaternion spaceRotation = _owner.transform.rotation * Quaternion.Euler(0f, _lookInput.x, 0f);

            foreach (var element in _settings.pitchOffsetElements)
            {
                float angle = sign ? element.clampedAngle.x : element.clampedAngle.y;
                Transform bone = _rigComponent.GetRigTransform(element.rigElement);
                
                bone.rotation = KAnimationMath.RotateInSpace(spaceRotation, bone.rotation,
                    Quaternion.Euler(angle * fraction, 0f, 0f), Weight);
            }
        }
    }
}