using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace TND.Upscaling.Framework.HDRP
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera), typeof(HDAdditionalCameraData))]
    public abstract class UpscalerController_HDRP : UpscalerController
    {
        protected internal override Vector2Int DisplaySize => new(_camera.pixelWidth, _camera.pixelHeight);
        protected internal override Vector2Int MaxRenderSize => DynamicResolutionHandler.instance.ApplyScalesOnSize(DisplaySize);

        private Camera _camera;
        private HDAdditionalCameraData _hdAdditionalCameraData;
        private GameObject _customPassGameObject;
        private OpaqueCopyPass _opaqueCopyPass;
        private AutoReactiveMaskPass _autoReactiveMaskPass;
        private FreezeJitterPass _freezeJitterPass;
        private PerformDynamicRes _scalerCallback;

        protected internal override Texture OpaqueOnlyTexture => _opaqueCopyPass?.OpaqueOnlyTexture;
        protected internal override Texture AutoReactiveMask => _autoReactiveMaskPass?.ReactiveMaskTexture;

        protected override void Awake()
        {
            base.Awake();

            _camera = GetComponent<Camera>();
            _hdAdditionalCameraData = GetComponent<HDAdditionalCameraData>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _camera = GetComponent<Camera>();
            _hdAdditionalCameraData = GetComponent<HDAdditionalCameraData>();
            _opaqueCopyPass = CreateOpaqueOnlyCustomPass();
            _autoReactiveMaskPass = CreateAutoReactiveMaskPass();
            _freezeJitterPass = CreateCustomJitterPass();
            
            _hdAdditionalCameraData.allowDeepLearningSuperSampling = true;

            // Perform run-time validations when in Play mode
            if (Application.isPlaying)
            {
                switch (RuntimeValidations.ValidateInjection())
                {
                    case RuntimeValidations.InjectionStatus.MissingNvidiaModuleDefines:
                    {
                        Debug.LogError(
@$"TND Upscaler is present and enabled on camera {name}, but the NVIDIA DLSS scripting defines are missing!
Make sure the NVIDIA module is correctly enabled in the Package Manager and that ENABLE_NVIDIA and ENABLE_NVIDIA_MODULE are defined on the HDRP assembly definition. 
The TND Upscaler script will now disable itself.");
                        enabled = false;
                        return;
                    }
                    case RuntimeValidations.InjectionStatus.DlssPassUsesNvidiaModule:
                    {
                        Debug.LogError(
@$"TND Upscaler is present and enabled on camera {name}, but the TND classes are not fully injected into HDRP yet!
Make sure the TND Upscaler framework is installed properly, and try recompiling scripts or restarting Unity.
If this error does not go away by itself, please try manually reimporting the Unity.RenderPipelines.HighDefinition.Runtime assembly definition. 
The TND Upscaler script will now disable itself.");
                        enabled = false;
                        return;
                    }
                }

                if (!RuntimeValidations.ValidateRenderPipelineSettings(((HDRenderPipelineAsset)GraphicsSettings.currentRenderPipeline).currentPlatformRenderPipelineSettings))
                {
                    Debug.LogError(
@$"TND Upscaler is present and enabled on camera {name}, but the current HDRP Render Pipeline Asset is incorrectly configured! 
Make sure you follow the TND Upscaler setup steps for the currently active Build Target, Quality Level and associated Render Pipeline Asset. 
The TND Upscaler script will now disable itself.");
                    enabled = false;
                    return;
                }

                if (!RuntimeValidations.ValidateCameraSettings(_camera, _hdAdditionalCameraData, this))
                {
                    Debug.LogError(
@$"TND Upscaler is present and enabled on camera {name}, but the camera itself is incorrectly configured! 
Make sure you follow the TND Upscaler setup steps for camera {name}. 
The TND Upscaler script will now disable itself.");
                    enabled = false;
                    return;
                }
            }

            if (_opaqueCopyPass != null)
                _opaqueCopyPass.enabled = EnableOpaqueOnlyCopy;

            if (_autoReactiveMaskPass != null)
                _autoReactiveMaskPass.enabled = autoGenerateReactiveMask;
            
            if (_freezeJitterPass != null)
                _freezeJitterPass.enabled = UpscalerContext?.ActiveUpscalerPlugin is not { IsTemporalUpscaler: true };
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _hdAdditionalCameraData.allowDeepLearningSuperSampling = false;
            
            if (_opaqueCopyPass != null)
                _opaqueCopyPass.enabled = false;

            if (_autoReactiveMaskPass != null)
                _autoReactiveMaskPass.enabled = false;
            
            if (_freezeJitterPass != null)
                _freezeJitterPass.enabled = false;

            if (_customPassGameObject != null)
            {
                CoreUtils.Destroy(_customPassGameObject);
                _customPassGameObject = null;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            if (qualityMode > UpscalerQuality.Custom)
            {
                _scalerCallback ??= () => 100f / GetScaleFactor(qualityMode);
                DynamicResolutionHandler.SetDynamicResScaler(_scalerCallback, DynamicResScalePolicyType.ReturnsPercentage);
                DynamicResolutionHandler.SetActiveDynamicScalerSlot(DynamicResScalerSlot.User);
            }

#if UNITY_2022_3_OR_NEWER
            if (GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset renderPipeline)
            {
                bool beforePost = injectionPoint < UpscalerInjectionPoint.AfterPostProcessing;
                var scheduleType = renderPipeline.currentPlatformRenderPipelineSettings.dynamicResolutionSettings.DLSSInjectionPoint;
                if ((beforePost && scheduleType != DynamicResolutionHandler.UpsamplerScheduleType.BeforePost) ||
                    (!beforePost && scheduleType != DynamicResolutionHandler.UpsamplerScheduleType.AfterPost))
                {
                    var pipelineSettings = renderPipeline.currentPlatformRenderPipelineSettings;
                    pipelineSettings.dynamicResolutionSettings.DLSSInjectionPoint = beforePost
                        ? DynamicResolutionHandler.UpsamplerScheduleType.BeforePost
                        : DynamicResolutionHandler.UpsamplerScheduleType.AfterPost;
                
                    renderPipeline.currentPlatformRenderPipelineSettings = pipelineSettings;
                }
            }
#endif
            
            if (_opaqueCopyPass != null)
                _opaqueCopyPass.enabled = EnableOpaqueOnlyCopy;

            if (_autoReactiveMaskPass != null)
            {
                var activeUpscalerPlugin = UpscalerContext?.ActiveUpscalerPlugin;
                _autoReactiveMaskPass.enabled = autoGenerateReactiveMask && (activeUpscalerPlugin?.AcceptsReactiveMask ?? false);
                _autoReactiveMaskPass.OpaqueCopyPass = _opaqueCopyPass;
                _autoReactiveMaskPass.AutoReactiveMaterial = UpscalerContext?.AutoReactiveMaterial;
                _autoReactiveMaskPass.AutoReactiveSettings = autoReactiveSettings;
                _autoReactiveMaskPass.ReactiveMaskFormat = activeUpscalerPlugin?.ReactiveMaskFormat ?? GraphicsFormat.R8_UNorm;
            }

            if (_freezeJitterPass != null)
                _freezeJitterPass.enabled = UpscalerContext?.ActiveUpscalerPlugin is not { IsTemporalUpscaler: true };
        }

        protected void ResetCameraHistory()
        {
            HDCamera.GetOrCreate(_camera).Reset();
        }

        private OpaqueCopyPass CreateOpaqueOnlyCustomPass()
        {
            return GetOrCreatePassOfType<OpaqueCopyPass>(CustomPassInjectionPoint.BeforeTransparent);
        }

        private AutoReactiveMaskPass CreateAutoReactiveMaskPass()
        {
            return GetOrCreatePassOfType<AutoReactiveMaskPass>(CustomPassInjectionPoint.BeforePostProcess);
        }

        private FreezeJitterPass CreateCustomJitterPass()
        {
            return GetOrCreatePassOfType<FreezeJitterPass>(CustomPassInjectionPoint.BeforeRendering);
        }

        private TPass GetOrCreatePassOfType<TPass>(CustomPassInjectionPoint passInjectionPoint)
            where TPass: CustomPass
        {
            CustomPassVolume customPassVolume = GetOrCreateCustomPassVolume(passInjectionPoint);
            foreach (var customPass in customPassVolume.customPasses)
            {
                if (customPass is TPass pass)
                    return pass;
            }

            return (TPass)customPassVolume.AddPassOfType(typeof(TPass));
        }
        
        private static readonly List<CustomPassVolume> CustomPassVolumes = new();

        private CustomPassVolume GetOrCreateCustomPassVolume(CustomPassInjectionPoint passInjectionPoint)
        {
            CustomPassVolume customPassVolume = null;
            
            _customPassGameObject = GetOrCreateCustomPassObject();
            
            _customPassGameObject.GetComponents(CustomPassVolumes);
            foreach (var volume in CustomPassVolumes)
            {
                if (volume.injectionPoint == passInjectionPoint)
                {
                    customPassVolume = volume;
                    break;
                }
            }

            CustomPassVolumes.Clear();

            if (customPassVolume == null)
            {
                customPassVolume = _customPassGameObject.AddComponent<CustomPassVolume>();
                customPassVolume.injectionPoint = passInjectionPoint;
            }

            return customPassVolume;
        }
        
        private GameObject GetOrCreateCustomPassObject()
        {
            if (_customPassGameObject == null)
            {
                _customPassGameObject = new GameObject("HDRP UpscalerController Custom Passes");
                _customPassGameObject.transform.parent = transform;
                _customPassGameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            return _customPassGameObject;
        }
    }
}
