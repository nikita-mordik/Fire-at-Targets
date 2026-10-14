using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace TND.Upscaling.Framework.HDRP
{
    public static class RuntimeValidations
    {
        private static readonly Type DlssPassType = Type.GetType("UnityEngine.Rendering.HighDefinition.DLSSPass, Unity.RenderPipelines.HighDefinition.Runtime", true);
        private static readonly FieldInfo DlssDeviceField = DlssPassType.GetField("m_Device", BindingFlags.Instance | BindingFlags.NonPublic);

        public enum InjectionStatus
        {
            MissingNvidiaModuleDefines,
            DlssPassUsesNvidiaModule,
            DlssPassUsesTndPackage,
        }
        
        public static InjectionStatus ValidateInjection()
        {
            if (DlssDeviceField == null)
            {
                // Device field only gets included when the ENABLE_NVIDIA defines are set, so it's possible that it doesn't exist yet
                return InjectionStatus.MissingNvidiaModuleDefines;
            }

            // After injecting references, HDRP's DLSSPass should be using classes from the TND Injectors assembly.
            return DlssDeviceField.FieldType.Assembly.FullName.StartsWith("TND", StringComparison.InvariantCulture) 
                ? InjectionStatus.DlssPassUsesTndPackage 
                : InjectionStatus.DlssPassUsesNvidiaModule;
        }

        /// <summary>
        /// Render pipeline validations that are critical and should produce an error when set incorrectly.
        /// </summary>
        public static bool ValidateRenderPipelineSettings(in RenderPipelineSettings renderPipelineSettings, StringBuilder errors = null)
        {
            bool result = true;
            
            // DLSS has enabled and needs top priority for the TND upscalers to be activated
            bool dlssEnabled = false;

#if UNITY_6000_3_OR_NEWER
            var dynamicResolutionSettings = renderPipelineSettings.dynamicResolutionSettings;
            if (dynamicResolutionSettings.advancedUpscalerNames != null && dynamicResolutionSettings.advancedUpscalerNames.Count > 0)
            {
                if (dynamicResolutionSettings.advancedUpscalerNames[0] == nameof(AdvancedUpscalers.DLSS))
                {
                    dlssEnabled = true;
                }
            }
#elif UNITY_2023_2_OR_NEWER
            var dynamicResolutionSettings = renderPipelineSettings.dynamicResolutionSettings;
            if (dynamicResolutionSettings.advancedUpscalersByPriority != null && dynamicResolutionSettings.advancedUpscalersByPriority.Count > 0)
            {
                if (dynamicResolutionSettings.advancedUpscalersByPriority[0] == AdvancedUpscalers.DLSS)
                {
                    dlssEnabled = true;
                }
            }
#else
            dlssEnabled = renderPipelineSettings.dynamicResolutionSettings.enableDLSS;
#endif

            if (!dlssEnabled)
            {
                errors?.AppendLine("- DLSS needs to be set as the topmost priority Advanced Upscaler in the HDRP settings.");
                result = false;
            }

            // Dynamic resolution needs to be enabled for upscaling to work at all
            if (!renderPipelineSettings.dynamicResolutionSettings.enabled)
            {
                errors?.AppendLine("- Dynamic resolution needs to be enabled in the HDRP settings.");
                result = false;
            }

            // DLSS Mode should be set to Maximum Quality. This doesn't really affect the upscalers directly,
            // but it mitigates some weird glitches in the Unity Editor with certain native plugin upscalers.
            if (renderPipelineSettings.dynamicResolutionSettings.DLSSPerfQualitySetting != 2)
            {
                errors?.AppendLine("- DLSS Mode needs to be set to Maximum Quality in the HDRP settings.");
                result = false;
            }

            // DLSS Optimal Settings will override the quality mode set on the TNDUpscaler script, so that needs to be disabled
            if (renderPipelineSettings.dynamicResolutionSettings.DLSSUseOptimalSettings)
            {
                errors?.AppendLine("- DLSS Use Optimal Settings needs to be disabled in the HDRP settings.");
                result = false;
            }

            // Force resolution needs to be disabled to allow different quality modes to be possible
            if (renderPipelineSettings.dynamicResolutionSettings.forceResolution)
            {
                errors?.AppendLine("- Force Screen Percentage needs to be disabled in the HDRP settings.");
                result = false;
            }

            // A minimum scale percentage of 33% is required to allow Ultra Performance mode (3.0x scale) to be possible
            if (renderPipelineSettings.dynamicResolutionSettings.minPercentage > 33)
            {
                errors?.AppendLine("- Minimum Screen Percentage needs to be set to 33 or lower in the HDRP settings.");
                result = false;
            }

            // A maximum scale percentage of 100% is required to allow Native AA mode (1.0x scale) to be possible
            if (renderPipelineSettings.dynamicResolutionSettings.maxPercentage < 100)
            {
                errors?.AppendLine("- Maximum Screen Percentage needs to be set to 100 in the HDRP settings.");
                result = false;
            }

            // Custom pass support is required for the opaque-only copy pass
            if (!renderPipelineSettings.supportCustomPass)
            {
                errors?.AppendLine("- Custom Pass support needs to be enabled in the HDRP settings.");
                result = false;
            }

            // MSAA should be disabled to prevent issues with unresolved render targets
            if (renderPipelineSettings.msaaSampleCount > MSAASamples.None)
            {
                errors?.AppendLine("- MSAA needs to be disabled in the HDRP settings.");
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Render pipeline validations that are not critical and should produce a warning when set incorrectly.
        /// </summary>
        public static bool ValidateRenderPipelineSettingsExtra(in RenderPipelineSettings renderPipelineSettings)
        {
            // Motion vectors need to be enabled for correct temporal reprojection
            if (!renderPipelineSettings.supportMotionVectors)
            {
                return false;
            }

            // The most basic upsample filter should be set to prevent interference with our upscalers
            if (renderPipelineSettings.dynamicResolutionSettings.upsampleFilter != DynamicResUpscaleFilter.CatmullRom)
            {
                return false;
            }

            // Dynamic resolution type should be set to Software for broadest compatibility
            if (renderPipelineSettings.dynamicResolutionSettings.dynResType != DynamicResolutionType.Software)
            {
                return false;
            }

            // Global mip bias should be enabled to keep textures sharp
            if (!renderPipelineSettings.dynamicResolutionSettings.useMipBias)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Camera setting validations that are critical and should produce an error when set incorrectly.
        /// </summary>
        public static bool ValidateCameraSettings(Camera camera, HDAdditionalCameraData additionalCameraData, UpscalerController upscalerController, StringBuilder errors = null)
        {
            bool result = true;
            
            // The camera should not override any of the HDRP pipeline settings:
            // Dynamic resolution needs to be enabled
            bool scriptEnabled = upscalerController != null && upscalerController.enabled;
            if (scriptEnabled && !additionalCameraData.allowDynamicResolution)
            {
                errors?.AppendLine("- Dynamic resolution needs to be enabled on the Camera.");
                result = false;
            }

            // DLSS needs to be enabled
            if (scriptEnabled && !additionalCameraData.allowDeepLearningSuperSampling)
            {
                errors?.AppendLine("- DLSS needs to be enabled on the Camera.");
                result = false;
            }

            // DLSS custom quality settings needs to be disabled
            if (additionalCameraData.deepLearningSuperSamplingUseCustomQualitySettings)
            {
                errors?.AppendLine("- DLSS Custom Quality needs to be disabled on the Camera.");
                result = false;
            }

            // DLSS custom attributes need to be disabled
            if (additionalCameraData.deepLearningSuperSamplingUseCustomAttributes)
            {
                errors?.AppendLine("- DLSS Custom Attributes needs to be disabled on the Camera.");
                result = false;
            }

            // DLSS optimal settings needs to be disabled, otherwise TNDUpscaler quality mode won't work
            if (additionalCameraData.deepLearningSuperSamplingUseOptimalSettings)
            {
                errors?.AppendLine("- DLSS Use Optimal Settings needs to be disabled on the Camera.");
                result = false;
            }
            
            // MSAA should be disabled to prevent issues with unresolved render targets
            if (camera.allowMSAA)
            {
                errors?.AppendLine("- MSAA needs to be disabled on the Camera.");
                result = false;
            }

            return result;
        }
        
        /// <summary>
        /// Camera setting validations that are not critical and should produce a warning when set incorrectly.
        /// </summary>
        public static bool ValidateCameraSettingsExtra(HDAdditionalCameraData additionalCameraData)
        {
            // Anti-aliasing should be disabled to prevent interference with our upscalers
            if (additionalCameraData.antialiasing != HDAdditionalCameraData.AntialiasingMode.None)
            {
                return false;
            }

            return true;
        }
    }
}
