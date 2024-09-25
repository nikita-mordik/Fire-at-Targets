// Designed by KINEMATION, 2024.

using KINEMATION.FPSAnimationFramework.Runtime.Camera;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.KAnimationCore.Runtime.Input;

using System;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Core
{
    [HelpURL("https://kinemation.gitbook.io/scriptable-animation-system/workflow/components")]
    public class FPSAnimator : MonoBehaviour
    {
        public bool HasLinkedProfile { get; private set; }

        [SerializeField] protected FPSAnimatorProfile animatorProfile;
        
        [NonSerialized] public IPlayablesController playablesController;
        [NonSerialized] protected FPSBoneController _boneController;
        [NonSerialized] protected UserInputController _inputController;
        [NonSerialized] protected FPSCameraController _cameraController;
        
        protected virtual void Start()
        {
            _boneController = GetComponent<FPSBoneController>();
            _inputController = GetComponent<UserInputController>();
            playablesController = GetComponent<IPlayablesController>();
            _cameraController = GetComponentInChildren<FPSCameraController>();

            _inputController.Initialize();
            playablesController.InitializeController();
            _boneController.Initialize();
            
            if(_cameraController != null) _cameraController.Initialize();
            
            _boneController.LinkAnimatorProfile(animatorProfile);
        }

        protected virtual void Update()
        {
            if (_boneController == null) return;
            _boneController.GameThreadUpdate();
        }

        protected virtual void LateUpdate()
        {
            if (_boneController == null && _cameraController != null)
            {
                _cameraController.UpdateCamera();
                return;
            }
            
            // Caches the active pose in case of blending.
            _boneController.CachePose();
            // Apply procedural animation.
            _boneController.EvaluatePose();
            // Blends in the cached pose.
            _boneController.ApplyCachedPose();
            
            if (_cameraController != null)
            {
                _cameraController.UpdateCamera();
            }
            
            _boneController.PostEvaluatePose();
        }

        protected virtual void OnDestroy()
        {
            if (_boneController == null) return;
            _boneController.Dispose();
        }
        
        public void UnlinkAnimatorProfile()
        {
            if (_boneController == null) return;
            
            _boneController.UnlinkAnimatorProfile();
            HasLinkedProfile = false;
        }

        public void LinkAnimatorProfile(GameObject itemEntity)
        {
            if (_boneController == null) return;
            
            if (itemEntity.GetComponent<FPSAnimatorEntity>() is var entity && entity != null)
            {
                LinkAnimatorProfile(entity.animatorProfile);
                _boneController.UpdateEntity(entity);
            }
        }

        public void LinkAnimatorProfile(FPSAnimatorProfile newProfile)
        {
            if (_boneController == null) return;
            
            _boneController.LinkAnimatorProfile(newProfile);
            HasLinkedProfile = true;
        }
        
        // Will force to dynamically link the layer via OnSettingsUpdated callback.
        public void LinkAnimatorLayer(FPSAnimatorLayerSettings newSettings)
        {
            if (_boneController == null) return;
            
            _boneController.LinkAnimatorLayer(newSettings);
        }
    }
}