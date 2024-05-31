// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.PoseSamplerLayer
{
    public class PoseSamplerLayerState : FPSAnimatorLayerState
    {
        private PoseSamplerLayerSettings _settings;

        private Transform _ikWeaponBone;
        private Transform _ikHandRight;
        private Transform _ikHandLeft;
        private Transform _ikHandRightHint;
        private Transform _ikHandLeftHint;
        
        private Transform _weaponBone;
        private Transform _weaponBoneRight;
        private Transform _weaponBoneLeft;

        private Transform _pelvis;
        private Transform _spineRoot;
        
        // In component space.
        private Quaternion _cachedPelvisPose;
        private KTransform _weaponBoneComponentPose;
        // In spine root bone space.
        private KTransform _weaponBoneSpinePose;

        private bool _isValidToEvaluate;
        private Quaternion _cachedSpinePose;

        private int _stabilizationWeightPropertyIndex;

        private void SamplePose(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (PoseSamplerLayerSettings) newSettings;

            if (_settings.poseToSample == null)
            {
                // Null pose means we have an empty layer.
                _isValidToEvaluate = false;
                return;
            }

            _isValidToEvaluate = true;
            
            // Get reference to the Hands IK.
            _ikWeaponBone = _rigComponent.GetRigTransform(_settings.ikWeaponBone);
            _ikHandRight = _rigComponent.GetRigTransform(_settings.ikHandRight);
            _ikHandLeft = _rigComponent.GetRigTransform(_settings.ikHandLeft);

            // Get reference to spine chain.
            _pelvis = _rigComponent.GetRigTransform(_settings.pelvis);
            _spineRoot = _rigComponent.GetRigTransform(_settings.spineRoot);
            
            // Get reference to weapon bones.
            _weaponBone = _rigComponent.GetRigTransform(_settings.weaponBone);
            _weaponBoneRight = _rigComponent.GetRigTransform(_settings.weaponBoneRight);
            _weaponBoneLeft = _rigComponent.GetRigTransform(_settings.weaponBoneLeft);
            
            // Override the weapon bone transform with static data.
            _weaponBone.position = _owner.transform.TransformPoint(_settings.defaultWeaponPose.position);
            _weaponBone.rotation = _owner.transform.rotation * _settings.defaultWeaponPose.rotation;
            
            // Try overriding with the animation.
            _settings.poseToSample.clip.SampleAnimation(_owner, 0f);
            
            _weaponBoneRight.position = _weaponBone.position;
            _weaponBoneRight.rotation = _weaponBone.rotation;

            _weaponBoneLeft.position = _weaponBoneRight.position;
            _weaponBoneLeft.rotation = _weaponBoneRight.rotation;
            
            _ikHandRightHint = _rigComponent.GetRigTransform(_settings.ikHandRightHint);
            _ikHandLeftHint = _rigComponent.GetRigTransform(_settings.ikHandLeftHint);

            // ReSharper disable all
            _cachedPelvisPose = Quaternion.Inverse(_owner.transform.rotation) * _pelvis.rotation;
            
            _weaponBoneComponentPose = 
                new KTransform(_owner.transform).GetRelativeTransform(new KTransform(_weaponBone), false);
            
            _weaponBoneSpinePose = 
                new KTransform(_spineRoot).GetRelativeTransform(new KTransform(_weaponBone), false);
            
            _playablesController.PlayPose(_settings.poseToSample);
            
            _weaponBone.rotation *= _settings.weaponBoneOffset.rotation;
            
            _ikWeaponBone.position = _weaponBone.position;
            _ikWeaponBone.rotation = _weaponBone.rotation;
        }

        private void ApplySpineStabilization()
        {
            // Stabilize spine first.
            float weight = _inputController.GetValue<float>(_stabilizationWeightPropertyIndex) * Weight;

            Quaternion rootRotation = _owner.transform.rotation;
            Quaternion pelvisWorldRotation = rootRotation * _cachedPelvisPose;
            Quaternion stabilizedSpineRotation = pelvisWorldRotation * _spineRoot.localRotation;

            Quaternion spineMS = Quaternion.Inverse(rootRotation) * _spineRoot.rotation;
            Quaternion stableMS = Quaternion.Inverse(rootRotation) * stabilizedSpineRotation;

            _cachedSpinePose = Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(spineMS) * stableMS, 
                weight);
            _spineRoot.rotation = Quaternion.Slerp(_spineRoot.rotation, stabilizedSpineRotation, weight);
        }

        private void AdjustWeaponBone()
        {
            float weaponBoneWeight = _playablesController.GetCurveValue(_settings.weaponBoneWeight);

            if (weaponBoneWeight > 0f)
            {
                KTransform componentPose
                    = new KTransform(_owner.transform).GetWorldTransform(_weaponBoneComponentPose, false);
                KTransform spinePose = new KTransform(_spineRoot).GetWorldTransform(_weaponBoneSpinePose, false);

                spinePose.position -= componentPose.position;
                spinePose.rotation = Quaternion.Inverse(componentPose.rotation) * spinePose.rotation;
                
                _weaponBone.position += spinePose.position;
                _weaponBone.rotation *= spinePose.rotation;
            }

            // -1, 0, 1.
            KTransform pose = new KTransform(_weaponBoneRight);
            KTransform poseRight = new KTransform(_weaponBone);
            KTransform poseLeft = new KTransform(_weaponBoneLeft);

            pose = weaponBoneWeight >= 0f
                ? KTransform.Lerp(pose, poseRight, weaponBoneWeight)
                : KTransform.Lerp(pose, poseLeft, -weaponBoneWeight);

            KTransform cachedRightHandIk = new KTransform(_ikHandRight);
            KTransform cachedLeftHandIk = new KTransform(_ikHandLeft);
            
            KTransform cachedRightHandIkHint = new KTransform(_ikHandRightHint);
            KTransform cachedLeftHandIkHint = new KTransform(_ikHandLeftHint);

            _ikWeaponBone.position = pose.position;
            _ikWeaponBone.rotation = pose.rotation * _settings.weaponBoneOffset.rotation;

            _ikHandRight.position = cachedRightHandIk.position;
            _ikHandRight.rotation = cachedRightHandIk.rotation;
            
            _ikHandLeft.position = cachedLeftHandIk.position;
            _ikHandLeft.rotation = cachedLeftHandIk.rotation;
            
            _ikHandRightHint.position = cachedRightHandIkHint.position;
            _ikHandRightHint.rotation = cachedRightHandIkHint.rotation;
            
            _ikHandLeftHint.position = cachedLeftHandIkHint.position;
            _ikHandLeftHint.rotation = cachedLeftHandIkHint.rotation;
        }
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            SamplePose(newSettings);
            _stabilizationWeightPropertyIndex = _inputController.GetPropertyIndex(_settings.stabilizationWeight);
        }

        public override void OnLayerLinked(FPSAnimatorLayerSettings newSettings)
        {
            SamplePose(newSettings);
        }

        public override void RegisterBones(ref HashSet<int> registeredBones)
        {
            registeredBones.Add(_settings.spineRoot.index);
        }

        public override void CachePoses(ref List<KPose> cachedPoses)
        {
            KPose spinePose = new KPose()
            {
                element = _settings.spineRoot,
                modifyMode = EModifyMode.Add,
                pose = new KTransform(Vector3.zero, _cachedSpinePose),
                space = ESpaceType.ComponentSpace
            };
            
            cachedPoses.Add(spinePose);
        }

        public override void OnGameThreadUpdate()
        {
            if (!_isValidToEvaluate)
            {
                return;
            }
            
            _weaponBone.position = _owner.transform.TransformPoint(_settings.defaultWeaponPose.position);
            _weaponBone.rotation = _owner.transform.rotation * _settings.defaultWeaponPose.rotation;
        }

        public override void OnEvaluatePose()
        {
            if (!_isValidToEvaluate) return;

            KTransform localRoot = new KTransform(_pelvis.parent, false);
            KTransform worldPelvis = new KTransform(_pelvis);

            if (_settings.overwriteRoot)
            {
                _pelvis.parent.localRotation = Quaternion.identity;
                _pelvis.parent.localPosition = Vector3.zero;
            }
            
            _pelvis.rotation = worldPelvis.rotation;
            _pelvis.position = worldPelvis.position;
            
            ApplySpineStabilization();
            AdjustWeaponBone();

            if (_settings.overwriteRoot)
            {
                _pelvis.parent.localRotation = localRoot.rotation;
                _pelvis.parent.localPosition = localRoot.position;
            }

            _pelvis.rotation = worldPelvis.rotation;
            _pelvis.position = worldPelvis.position;
        }
    }
}