// Designed by KINEMATION, 2024

using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Core;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace KINEMATION.FPSAnimationFramework.Runtime.Layers.IkLayer
{
    public class IkLayerState : FPSAnimatorLayerState
    {
        private Transform _rightHand;
        private Transform _leftHand;
        
        private Transform _rightHandIk;
        private Transform _leftHandIk;
        
        private Transform _rightHandHint;
        private Transform _leftHandHint;
        
        private Transform _rightFoot;
        private Transform _leftFoot;
        
        private Transform _rightFootIk;
        private Transform _leftFootIk;
        
        private Transform _rightFootHint;
        private Transform _leftFootHint;

        private IkLayerSettings _settings;
        
        // Two Bone IK Job
        private JobHandle _ikJobHandle;
        private NativeArray<KTwoBoneIkData> _ikJobDataArray;

        private void PrepareIkElement(Transform tip, Transform hint, Transform target, int index)
        {
            KTwoBoneIkData twoBoneIkData = new KTwoBoneIkData()
            {
                posWeight = 0f,
                rotWeight = 0f,
                hintWeight = 0f,
            };

            if (tip != null && target != null)
            {
                Transform mid = tip.parent;

                twoBoneIkData.root = new KTransform(mid.parent); 
                twoBoneIkData.mid = new KTransform(mid);
                twoBoneIkData.tip = new KTransform(tip);

                twoBoneIkData.target = new KTransform(target);
                twoBoneIkData.hint = new KTransform(hint == null ? mid : hint);

                twoBoneIkData.hasValidHint = true;
                twoBoneIkData.posWeight = _settings.alpha;
                twoBoneIkData.rotWeight = _settings.alpha;
                twoBoneIkData.hintWeight = _settings.alpha;
            }

            _ikJobDataArray[index] = twoBoneIkData;
        }

        private void ApplyIk(Transform tip, KTwoBoneIkData twoBoneIkData)
        {
            Transform mid = tip.parent;

            mid.parent.rotation = twoBoneIkData.root.rotation;
            mid.rotation = twoBoneIkData.mid.rotation;
            tip.rotation = twoBoneIkData.tip.rotation;
        }
        
        public override void InitializeState(FPSAnimatorLayerSettings newSettings)
        {
            _settings = (IkLayerSettings) newSettings;

            _rightHand = _rigComponent.GetRigTransform(_settings.rightHand);
            _leftHand = _rigComponent.GetRigTransform(_settings.leftHand);
            
            _rightHandIk = _rigComponent.GetRigTransform(_settings.rightHandIk);
            _leftHandIk = _rigComponent.GetRigTransform(_settings.leftHandIk);
            
            _rightFoot = _rigComponent.GetRigTransform(_settings.rightFoot);
            _leftFoot = _rigComponent.GetRigTransform(_settings.leftFoot);
            
            _rightFootIk = _rigComponent.GetRigTransform(_settings.rightFootIk);
            _leftFootIk = _rigComponent.GetRigTransform(_settings.leftFootIk);
            
            _rightHandHint = _rigComponent.GetRigTransform(_settings.rightHandHint);
            _leftHandHint = _rigComponent.GetRigTransform(_settings.leftHandHint);
            
            _rightFootHint = _rigComponent.GetRigTransform(_settings.rightFootHint);
            _leftFootHint = _rigComponent.GetRigTransform(_settings.leftFootHint);
            
            // We will always be running IK, so no need to do anything here.
            _ikJobDataArray = new NativeArray<KTwoBoneIkData>(4, Allocator.Persistent);
        }

        public override void OnEvaluatePose()
        {
            PrepareIkElement(_rightHand, _rightHandHint, _rightHandIk, 0);
            PrepareIkElement(_leftHand, _leftHandHint, _leftHandIk, 1);
            PrepareIkElement(_rightFoot, _rightFootHint, _rightFootIk, 2);
            PrepareIkElement(_leftFoot, _leftFootHint, _leftFootIk, 3);

            var job = new KTwoBoneIKJob()
            {
                twoBoneIkJobData = _ikJobDataArray,
            };
            
            _ikJobHandle = job.Schedule(4, 1);
            _ikJobHandle.Complete();

            ApplyIk(_rightHand, _ikJobDataArray[0]);
            ApplyIk(_leftHand, _ikJobDataArray[1]);
            ApplyIk(_rightFoot, _ikJobDataArray[2]);
            ApplyIk(_leftFoot, _ikJobDataArray[3]);
        }

        public override void OnDestroyed()
        {
            if (!_ikJobHandle.IsCompleted)
            {
                _ikJobHandle.Complete();
            }
            
            _ikJobDataArray.Dispose();
        }
    }
}