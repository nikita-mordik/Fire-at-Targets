// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.SwayLayer
{
    public struct VectorSpringState
    {
        public FloatSpringState X;
        public FloatSpringState Y;
        public FloatSpringState Z;

        public void Reset()
        {
            X.Reset();
            Y.Reset();
            Z.Reset();
        }
    }
    
    public class SwayLayerState : FPSAnimatorLayerState
    {
        private SwayLayerSettings _settings;
        private Transform _boneToModify;
        private Transform _headBone;
        
        private Vector4 _mouseDelta;

        // Free aiming.
        private Vector2 _freeAimTarget;
        private Vector2 _freeAimValue;
        
        // Move sway.
        private Vector3 _moveSwayPositionTarget;
        private Vector3 _moveSwayRotationTarget;
        
        private Vector3 _moveSwayPositionResult;
        private Vector3 _moveSwayRotationResult;
        
        private VectorSpringState _moveSwayPositionSpring;
        private VectorSpringState _moveSwayRotationSpring;

        // Aim sway.
        private Vector2 _aimSwayTarget;
        private Vector3 _aimSwayPositionResult;
        private Vector3 _aimSwayRotationResult;

        private VectorSpringState _aimSwayPositionSpring;
        private VectorSpringState _aimSwayRotationSpring;

        private int _freeAimPropertyIndex;
        private int _moveInputPropertyIndex;
        private int _mouseInputPropertyIndex;

        private void VectorSpringInterp(ref Vector3 current, in Vector3 target, in VectorSpring spring, 
            ref VectorSpringState state)
        {
            current.x = KSpringMath.FloatSpringInterp(current.x, target.x, spring.speed.x, spring.damping.x,
                spring.stiffness.x, spring.scale.x, ref state.X);
            
            current.y = KSpringMath.FloatSpringInterp(current.y, target.y, spring.speed.y, spring.damping.y,
                spring.stiffness.y, spring.scale.y, ref state.Y);
            
            current.z = KSpringMath.FloatSpringInterp(current.z, target.z, spring.speed.z, spring.damping.z,
                spring.stiffness.z, spring.scale.z, ref state.Z);
        }
        
        private void EvaluateFreeAim()
        {
            bool useFreeAim = _inputController.GetValue<bool>(_freeAimPropertyIndex);

            if (useFreeAim)
            {
                // Accumulate the input.
                _freeAimTarget.x += _mouseDelta.x;
                _freeAimTarget.y += _mouseDelta.y;
                _freeAimTarget *= _settings.freeAimInputScale;

                // Clamp the user input.
                _freeAimTarget.x = Mathf.Clamp(_freeAimTarget.x, -_settings.freeAimClamp, 
                    _settings.freeAimClamp);
            
                _freeAimTarget.y = Mathf.Clamp(_freeAimTarget.y, -_settings.freeAimClamp, 
                    _settings.freeAimClamp);
            }
            else
            {
                _freeAimTarget = Vector2.zero;
            }

            // Finally interpolate the value.
            _freeAimValue = Vector2.Lerp(_freeAimValue, _freeAimTarget,
                KMath.ExpDecayAlpha(_settings.freeAimInterpSpeed, Time.deltaTime));
            
            Quaternion rotation = Quaternion.Euler(new Vector3(_freeAimValue.y, _freeAimValue.x, 0f));
            rotation.Normalize();

            Vector3 headMS = _owner.transform.InverseTransformPoint(_headBone.position);
            Vector3 masterMS = _owner.transform.InverseTransformPoint(_boneToModify.position);

            Vector3 offset = headMS - masterMS;
            offset = rotation * offset - offset;

            KTransform freeAimTransform = new KTransform()
            {
                position = -offset,
                rotation = rotation
            };

            KPose pose = new KPose()
            {
                modifyMode = EModifyMode.Add,
                pose = freeAimTransform,
                space = _settings.freeAimSpace
            };
            
            KAnimationMath.ModifyTransform(_owner.transform, _boneToModify, pose, Weight);
        }

        private void EvaluateMoveSway()
        {
            Vector4 moveInput = _inputController.GetValue<Vector4>(_moveInputPropertyIndex);

            var rotationTarget = new Vector3()
            {
                x = moveInput.y,
                y = moveInput.x,
                z = moveInput.x
            };

            var positionTarget = new Vector3()
            {
                x = moveInput.x,
                y = moveInput.y,
                z = moveInput.y
            };

            float alpha = KMath.ExpDecayAlpha(_settings.moveSwayTargetDamping, Time.deltaTime);
            
            _moveSwayPositionTarget = Vector3.Lerp(_moveSwayPositionTarget, positionTarget / 100f, alpha);
            _moveSwayRotationTarget = Vector3.Lerp(_moveSwayRotationTarget, rotationTarget, alpha);

            VectorSpringInterp(ref _moveSwayPositionResult,
                _moveSwayPositionTarget, _settings.moveSwayPositionSpring, ref _moveSwayPositionSpring);

            VectorSpringInterp(ref _moveSwayRotationResult,
                _moveSwayRotationTarget, _settings.moveSwayRotationSpring, ref _moveSwayRotationSpring);

            KTransform transform = new KTransform()
            {
                position = _moveSwayPositionResult,
                rotation = Quaternion.Euler(_moveSwayRotationResult).normalized,
                scale = Vector3.one
            };
            
            KPose pose = new KPose()
            {
                modifyMode = EModifyMode.Add,
                pose = transform,
                space = _settings.moveSwaySpace
            };
            
            KAnimationMath.ModifyTransform(_owner.transform, _boneToModify, pose, Weight);
        }

        private void EvaluateAimSway()
        {
            float deltaRight = _mouseDelta.x;
            float deltaUp = _mouseDelta.y;
            
            _aimSwayTarget += new Vector2(deltaRight, deltaUp) * 0.01f;

            float alpha = KMath.ExpDecayAlpha(_settings.aimSwayTargetDamping, Time.deltaTime);
            _aimSwayTarget = Vector2.Lerp(_aimSwayTarget, Vector2.zero, alpha);

            Vector3 targetLoc = new Vector3()
            {
                x = _aimSwayTarget.x,
                y = _aimSwayTarget.y,
                z = 0f
            };
            
            Vector3 targetRot = new Vector3()
            {
                x = _aimSwayTarget.y,
                y = _aimSwayTarget.x,
                z = _aimSwayTarget.x
            };

            VectorSpringInterp(ref _aimSwayPositionResult,
                targetLoc / 100f, _settings.aimSwayPositionSpring, ref _aimSwayPositionSpring);

            VectorSpringInterp(ref _aimSwayRotationResult,
                targetRot, _settings.aimSwayRotationSpring, ref _aimSwayRotationSpring);

            KTransform aimSwayTransform = new KTransform()
            {
                position = _aimSwayPositionResult,
                rotation = Quaternion.Euler(_aimSwayRotationResult)
            };
            
            KPose pose = new KPose()
            {
                modifyMode = EModifyMode.Add,
                pose = aimSwayTransform,
                space = _settings.aimSwaySpace
            };
            
            KAnimationMath.ModifyTransform(_owner.transform, _boneToModify, pose, Weight);
        }

        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (SwayLayerSettings) newSettings;
            _boneToModify = _rigComponent.GetRigTransform(_settings.boneToModify);
            _headBone = _rigComponent.GetRigTransform(_settings.headBone);

            _moveSwayPositionSpring.Reset();
            _moveSwayRotationSpring.Reset();

            _aimSwayPositionSpring.Reset();
            _aimSwayRotationSpring.Reset();

            _freeAimPropertyIndex = _inputController.GetPropertyIndex(_settings.useFreeAimProperty);
            _moveInputPropertyIndex = _inputController.GetPropertyIndex(_settings.moveInputProperty);
            _mouseInputPropertyIndex = _inputController.GetPropertyIndex(_settings.mouseDeltaInputProperty);
        }

        public override void OnEvaluatePose()
        {
            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                return;
            }
            
            _mouseDelta = _inputController.GetValue<Vector4>(_mouseInputPropertyIndex);
            _mouseDelta *= Time.timeScale;

            EvaluateFreeAim();
            EvaluateMoveSway();
            EvaluateAimSway();
        }
    }
}