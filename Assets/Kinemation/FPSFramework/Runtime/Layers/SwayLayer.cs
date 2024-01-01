// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public struct SwayLayerData
    {
        public Vector2 freeAimTarget;
        public Vector2 freeAimResult;

        public Vector2 aimSwayTarget;

        public VectorSpringState aimSwayPositionSpring;
        public VectorSpringState aimSwayRotationSpring;
        
        public Vector3 aimSwayPositionResult;
        public Vector3 aimSwayRotationResult;
        
        public Vector3 moveSwayRotationTarget;
        public Vector3 moveSwayPositionTarget;

        public VectorSpringState moveSwayPositionSpring;
        public VectorSpringState moveSwayRotationSpring;

        public Vector3 moveSwayPositionResult;
        public Vector3 moveSwayRotationResult;
    }

    public struct SwayLayerInputData
    {
        public float deltaTime;
        public bool useCircleMethod;

        public Vector2 aimInput;
        public Vector2 moveInput;

        public FreeAimData freeAimSettings;
        public MoveSwayData moveSwaySettings;
        public LocRotSpringData aimSwaySettings;
    }

    struct SwayLayerJob : IJob
    {
        public SwayLayerInputData inputData;
        public NativeArray<SwayLayerData> swayData;

        public void Execute()
        {
            var data = swayData[0];
            SwayLayer.ApplySway(ref inputData, ref data);
            SwayLayer.ApplyMoveSway(ref inputData, ref data);
            SwayLayer.ApplyFreeAim(ref inputData, ref data);
            swayData[0] = data;
        }
    }
    
    public class SwayLayer : AnimLayer
    {
        [Header("Deadzone Rotation")] [SerializeField] [Bone]
        protected Transform headBone;
        
        [SerializeField] protected bool bFreeAim = true;
        [SerializeField] protected bool useCircleMethod;

        private SwayLayerInputData _layerInput;
        private SwayLayerData _layerData;

        private JobHandle _jobHandle;
        private NativeArray<SwayLayerData> _jobData;
        
        public void SetFreeAimEnable(bool enable)
        {
            bFreeAim = enable;
        }

        public override void OnPoseSampled()
        {
            _layerData.aimSwayPositionSpring.Reset();
            _layerData.aimSwayRotationSpring.Reset();
            
            _layerData.moveSwayPositionSpring.Reset();
            _layerData.moveSwayRotationSpring.Reset();
            
            _layerInput.freeAimSettings = GetGunAsset().freeAimSettings;
            _layerInput.moveSwaySettings = GetGunAsset().moveSwaySettings;
            _layerInput.aimSwaySettings = GetGunAsset().aimSwaySettings;

            _layerData.aimSwayTarget = Vector2.zero;
            _layerData.moveSwayPositionResult = _layerData.moveSwayRotationResult = Vector3.zero;
            _layerData.moveSwayPositionTarget = _layerData.moveSwayRotationTarget = Vector3.zero;
        }

        protected void ApplyTransforms()
        {
            float alpha = CoreToolkitLib.ExpDecay(GetGunAsset().freeAimSettings.interpolationSpeed, Time.deltaTime);

            if (!bFreeAim) _layerData.freeAimTarget = Vector2.zero;
            _layerData.freeAimResult = Vector2.Lerp(_layerData.freeAimResult, _layerData.freeAimTarget, alpha);
            
            Quaternion q =
                Quaternion.Euler(new Vector3(_layerData.freeAimResult.x, _layerData.freeAimResult.y, 0f));
            q.Normalize();

            Vector3 headMS = GetRootBone().InverseTransformPoint(headBone.position);
            Vector3 masterMS = GetRootBone().InverseTransformPoint(GetMasterPivot().position);

            Vector3 offset = headMS - masterMS;
            offset = q * offset - offset;

            LocRot swayResult = new LocRot()
            {
                position = -offset,
                rotation = q
            };
            
            Quaternion aimSwayRotation = Quaternion.Euler(_layerData.aimSwayRotationResult);
            Vector3 aimSwayPosition = _layerData.aimSwayPositionResult;
            
            Vector3 swayOffset = GetGunAsset().adsSwayOffset * GetRigData().aimWeight;
            swayOffset = aimSwayRotation * swayOffset - swayOffset;
            aimSwayPosition += swayOffset;
            
            swayResult.position += aimSwayPosition + _layerData.moveSwayPositionResult;
            swayResult.rotation *= aimSwayRotation;
            swayResult.rotation *= Quaternion.Euler(_layerData.moveSwayRotationResult);
            
            GetMasterIK().Offset(GetRootBone(), swayResult.position, smoothLayerAlpha);
            GetMasterIK().Offset(GetRootBone(), swayResult.rotation, smoothLayerAlpha);
        }

        public override void PreUpdateLayer()
        {
            base.PreUpdateLayer();
            
            _layerInput.deltaTime = Time.deltaTime;

            _layerInput.aimInput = GetCharData().deltaAimInput;
            _layerInput.moveInput = GetCharData().moveInput;
            
            _layerInput.useCircleMethod = useCircleMethod;
        }

        public override bool CanUseParallelExecution()
        {
            return true;
        }

        public override void ScheduleJobs()
        {
            _jobData[0] = _layerData;
            
            var job = new SwayLayerJob()
            {
                inputData = _layerInput,
                swayData = _jobData
            };
            
            _jobHandle = job.Schedule();
        }

        public override void CompleteJobs()
        {
            _jobHandle.Complete();
            _layerData = _jobData[0];
            
            ApplyTransforms();
        }

        private void OnDestroy()
        {
            if (_jobData.IsCreated) _jobData.Dispose();
        }

        public override void InitializeLayer()
        {
            base.InitializeLayer();
            if (Application.isPlaying) _jobData = new NativeArray<SwayLayerData>(1, Allocator.Persistent);
        }

        public override void UpdateLayer()
        {
            ApplySway(ref _layerInput, ref _layerData);
            ApplyMoveSway(ref _layerInput, ref _layerData);
            ApplyFreeAim(ref _layerInput, ref _layerData);
            
            ApplyTransforms();
        }

        public static void ApplyFreeAim(ref SwayLayerInputData input, ref SwayLayerData data)
        {
            data.freeAimTarget.x += input.aimInput.y * input.freeAimSettings.inputScale;
            data.freeAimTarget.y += input.aimInput.x * input.freeAimSettings.inputScale;

            float maxValue = input.freeAimSettings.maxValue;
            data.freeAimTarget.x = Mathf.Clamp(data.freeAimTarget.x, -maxValue, maxValue);

            if (input.useCircleMethod)
            {
                var maxY = Mathf.Sqrt(Mathf.Pow(maxValue, 2f) - Mathf.Pow(data.freeAimTarget.x, 2f));
                data.freeAimTarget.y = Mathf.Clamp(data.freeAimTarget.y, -maxY, maxY);
            }
            else
            {
                data.freeAimTarget.y = Mathf.Clamp(data.freeAimTarget.y, 
                    -maxValue, maxValue);
            }
        }

        public static void ApplySway(ref SwayLayerInputData input, ref SwayLayerData data)
        {
            float deltaTime = input.deltaTime;
            
            float deltaRight = input.aimInput.x / deltaTime;
            float deltaUp = input.aimInput.y / deltaTime;
            
            data.aimSwayTarget += new Vector2(deltaRight, deltaUp) * 0.01f;
            data.aimSwayTarget.x = CoreToolkitLib.InterpLayer(data.aimSwayTarget.x * 0.01f, 0f, 5f, deltaTime);
            data.aimSwayTarget.y = CoreToolkitLib.InterpLayer(data.aimSwayTarget.y * 0.01f, 0f, 5f, deltaTime);

            Vector3 targetLoc = new Vector3()
            {
                x = data.aimSwayTarget.x,
                y = data.aimSwayTarget.y,
                z = 0f
            };
            
            Vector3 targetRot = new Vector3()
            {
                x = data.aimSwayTarget.y,
                y = data.aimSwayTarget.x,
                z = data.aimSwayTarget.x
            };

            data.aimSwayPositionResult = CoreToolkitLib.SpringInterp(data.aimSwayPositionResult, targetLoc,
                ref input.aimSwaySettings.loc, ref data.aimSwayPositionSpring, deltaTime);

            data.aimSwayRotationResult = CoreToolkitLib.SpringInterp(data.aimSwayRotationResult, targetRot,
                ref input.aimSwaySettings.rot, ref data.aimSwayRotationSpring, deltaTime);
        }

        public static void ApplyMoveSway(ref SwayLayerInputData input, ref SwayLayerData data)
        {
            float deltaTime = input.deltaTime;
            var moveSwayData = input.moveSwaySettings;
            var moveInput = input.moveInput;

            var moveRotTarget = new Vector3()
            {
                x = moveInput.y * moveSwayData.rotationScale.x,
                y = moveInput.x * moveSwayData.rotationScale.y,
                z = moveInput.x * moveSwayData.rotationScale.z
            };

            var moveLocTarget = new Vector3()
            {
                x = moveInput.x * moveSwayData.translationScale.x,
                y = moveInput.y * moveSwayData.translationScale.y,
                z = moveInput.y * moveSwayData.translationScale.z
            };

            data.moveSwayRotationTarget = CoreToolkitLib.Interp(data.moveSwayRotationTarget, 
                moveRotTarget, moveSwayData.rotationDampingFactor, deltaTime);
            
            data.moveSwayPositionTarget = CoreToolkitLib.Interp(data.moveSwayPositionTarget, 
                moveLocTarget, moveSwayData.translationDampingFactor, deltaTime);

            data.moveSwayRotationResult = CoreToolkitLib.SpringInterp(data.moveSwayRotationResult,
                data.moveSwayRotationTarget, ref moveSwayData.rotationSpringSettings,
                ref data.moveSwayRotationSpring, deltaTime);

            data.moveSwayPositionResult = CoreToolkitLib.SpringInterp(data.moveSwayPositionResult,
                data.moveSwayPositionTarget, ref moveSwayData.positionSpringSettings,
                ref data.moveSwayPositionSpring, deltaTime);
        }
    }
}