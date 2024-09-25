// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.AttachHandLayer
{
    public class AttachHandLayerState : FPSAnimatorLayerState
    {
        private AttachHandLayerSettings _settings;
        
        private Transform _handBone;
        private Transform _ikHandBone;
        private Transform _ikWeaponBone;
        
        private Transform _weaponBone;
        private KTransform _handPose = KTransform.Identity;
        
        private KTransformChain _leftHandChain;

        private void RefreshSettings(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (AttachHandLayerSettings) newSettings;
            
            _handBone = _rigComponent.GetRigTransform(_settings.handBone);
            _ikHandBone = _rigComponent.GetRigTransform(_settings.ikHandBone);
            _ikWeaponBone = _rigComponent.GetRigTransform(_settings.ikWeaponBone);
            _weaponBone = _rigComponent.GetRigTransform(_settings.weaponBone);

            bool hasValidCustomPose = _settings.customHandPose != null;
            
            if (hasValidCustomPose)
            {
                _rigComponent.CacheHierarchyPose();
                _settings.customHandPose.SampleAnimation(_owner, 0f);
            }
            
            _handPose = new KTransform(_weaponBone).GetRelativeTransform(new KTransform(_handBone), false);
            
            _leftHandChain = _settings.GetRigAsset().GetPopulatedChain(_settings.elementChainName, _rigComponent);
            _leftHandChain.CacheTransforms(ESpaceType.ParentBoneSpace);

            if (hasValidCustomPose)
            {
                _rigComponent.ApplyHierarchyCachedPose();
            }
        }
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            RefreshSettings(newSettings);
        }

        public override void OnLayerLinked(FPSAnimatorLayerSettings newSettings)
        {
            RefreshSettings(newSettings);
        }

        public override void RegisterBones(ref HashSet<int> registeredBones)
        {
            registeredBones.Add(_settings.handBone.index - 2);
            registeredBones.Add(_settings.handBone.index - 1);
            registeredBones.Add(_settings.handBone.index);
        }

        public override void CachePoses(ref List<KPose> cachedPoses)
        {
            Transform mid = _handBone.parent;
            Transform root = mid.parent;
            
            cachedPoses.Add(new KPose()
            {
                element = new KRigElement(_settings.handBone.index - 2, ""),
                modifyMode = EModifyMode.Replace,
                pose = new KTransform(_owner.transform).GetRelativeTransform(new KTransform(root), false),
                space = ESpaceType.ComponentSpace
            });
            
            cachedPoses.Add(new KPose()
            {
                element = new KRigElement(_settings.handBone.index - 1, ""),
                modifyMode = EModifyMode.Replace,
                pose = new KTransform(_owner.transform).GetRelativeTransform(new KTransform(mid), false),
                space = ESpaceType.ComponentSpace
            });
            
            cachedPoses.Add(new KPose()
            {
                element = _settings.handBone,
                modifyMode = EModifyMode.Replace,
                pose = new KTransform(_owner.transform).GetRelativeTransform(new KTransform(_handBone), false),
                space = ESpaceType.ComponentSpace
            });
        }

        public override void OnEvaluatePose()
        {
            KTransform attachedPose = new KTransform();
            
            attachedPose.position 
                = _ikWeaponBone.TransformPoint(_handPose.position + _settings.handPoseOffset.position);
            
            attachedPose.rotation 
                = _ikWeaponBone.rotation * (_handPose.rotation * _settings.handPoseOffset.rotation);
            
            _ikHandBone.position = Vector3.Lerp(_ikHandBone.position, attachedPose.position, Weight);
            _ikHandBone.rotation = Quaternion.Slerp(_ikHandBone.rotation, attachedPose.rotation, Weight);
            _leftHandChain.BlendTransforms(Weight);
        }

#if UNITY_EDITOR
        public override void OnSceneGUI()
        {
            Vector3 handlePos = _ikHandBone.position;
            Quaternion handleRot = _ikHandBone.rotation;
           
            if (Tools.current == Tool.Move)
            {
                handlePos = Handles.PositionHandle(handlePos, _ikWeaponBone.rotation);
                
                Vector3 localHandle = _ikWeaponBone.InverseTransformPoint(handlePos);
                Vector3 localHand = _ikWeaponBone.InverseTransformPoint(_ikHandBone.position);
            
                _settings.handPoseOffset.position += localHandle - localHand;
            }
            else if (Tools.current == Tool.Rotate)
            {
                handleRot = Handles.RotationHandle(handleRot, handlePos);
                
                Quaternion weaponInverse = Quaternion.Inverse(_ikWeaponBone.rotation);
                Quaternion localHandle = weaponInverse * handleRot;
                _settings.handPoseOffset.rotation
                    *= Quaternion.Inverse(weaponInverse * _ikHandBone.rotation) * localHandle;
            }
        }
#endif
    }
}