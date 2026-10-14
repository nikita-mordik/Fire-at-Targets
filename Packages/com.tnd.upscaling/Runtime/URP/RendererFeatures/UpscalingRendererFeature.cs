using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TND.Common.URP;

namespace TND.Upscaling.Framework.URP
{
    [DisallowMultipleRendererFeature("TND Upscaling")]
    public class UpscalingRendererFeature : ScriptableRendererFeature
    {
        public Shader copyDepthShader;
        public Shader upsampleDepthShader;

        private UniversalRenderPipelineAsset _renderPipelineAsset;
        private bool _usingRenderGraph;

        private UpscalingRenderPass _upscalingRenderPass;
        private OpaqueCopyPass _opaqueCopyPass;
        private AutoReactiveMaskPass _autoReactiveMaskPass;
        private CustomReactiveMaskPass _customReactiveMaskPass;
        private SetupPass _setupPass;
        private CleanupPass _cleanupPass;
        private Material _copyDepthMaterial;
        private Material _upsampleDepthMaterial;

        public override void Create()
        {
            // Prevent leaks and duplicate event registrations
            Dispose(true);
            
#if UNITY_EDITOR
            if (copyDepthShader == null)
            {
                const string path = "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepth.shader";
                copyDepthShader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(path);
            }
            
            if (upsampleDepthShader == null)
            {
                const string path = "Packages/com.tnd.upscaling/Runtime/URP/Shaders/UpsampleDepth.shader";
                upsampleDepthShader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(path);
            }
#endif
            
            name = "TND Upscaling";
            _upscalingRenderPass = new UpscalingRenderPass();
            _opaqueCopyPass = new OpaqueCopyPass();
            _autoReactiveMaskPass = new AutoReactiveMaskPass();
            _customReactiveMaskPass = new CustomReactiveMaskPass();
            _setupPass = new SetupPass();
            _cleanupPass = new CleanupPass();
            _copyDepthMaterial = new Material(copyDepthShader);
            _upsampleDepthMaterial = new Material(upsampleDepthShader);

            AddBeginCameraRenderingDelegate();
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
        }

        /// <summary>
        /// We need to guarantee that upscaling is the first listener to be invoked before camera rendering starts, because we modify the render scale.
        /// Any other listeners that make use of render scale to allocate render textures need to come after, otherwise they will use incorrect resolution values.
        /// Since Unity calls Awake on game object scripts before initializing renderer features, we cannot be sure in what order event listeners are registered and invoked.
        /// This leaves us with no choice but to use reflection to manually reorder the event invocation list, guaranteeing that we come first. 
        /// </summary>
        private void AddBeginCameraRenderingDelegate()
        {
            var eventInfo = RenderPipelineManagerMembers.BeginCameraRenderingEvent;
            var eventFieldValue = (Delegate)RenderPipelineManagerMembers.BeginCameraRenderingField.GetValue(null);

            // Remove all current listeners from the event
            var invocationList = eventFieldValue?.GetInvocationList();
            if (invocationList != null)
            {
                foreach (var listener in invocationList)
                {
                    eventInfo.RemoveEventHandler(null, listener);
                }
            }

            // Add our event listener at the start of the invocation list
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            
            // Re-add all the other listeners back to the event
            if (invocationList != null)
            {
                foreach (var listener in invocationList)
                {
                    eventInfo.AddEventHandler(null, listener);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;

            if (_upsampleDepthMaterial != null)
            {
                CoreUtils.Destroy(_upsampleDepthMaterial);
                _upsampleDepthMaterial = null;
            }

            if (_opaqueCopyPass != null)
            {
                _opaqueCopyPass.Dispose();
                _opaqueCopyPass = null;
            }

            if (_autoReactiveMaskPass != null)
            {
                _autoReactiveMaskPass.Dispose();
                _autoReactiveMaskPass = null;
            }

            if (_customReactiveMaskPass != null)
            {
                _customReactiveMaskPass.Dispose();
                _customReactiveMaskPass = null;
            }

            if (_upscalingRenderPass != null)
            {
                _upscalingRenderPass.Dispose();
                _upscalingRenderPass = null;
            }
            
            UpscalingHelpers.CleanupRTHandles();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!Application.isPlaying || !isActive)
            {
                return;
            }

            _renderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (_renderPipelineAsset == null)
            {
                return;
            }

            ref CameraData cameraData = ref renderingData.cameraData;
            if (!cameraData.resolveFinalTarget)
            {
                // Only perform upscaling on the final camera in a stack
                return;
            }
            
            Camera camera = cameraData.camera;
            if (camera.cameraType is not (CameraType.Game or CameraType.VR))
            {
                // Only perform upscaling on game cameras
                return;
            }
            
            // Ensure stale resources created by the upscaling passes don't stick around for too long
            UpscalingHelpers.PurgeUnusedRTHandles();

            if (!camera.TryGetComponent(out UpscalerController_URP controller) || !controller.enabled)
            {
                // Only perform upscaling when there's an active script attached to this camera
                return;
            }
            
            _usingRenderGraph =
#if TND_URP_RENDERGRAPH && TND_URP_COMPATIBILITY
                !GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode;
#elif TND_URP_RENDERGRAPH
                true;
#else
                false;
#endif
            
            Vector2Int maxRenderSize = controller.MaxRenderSize;
            Vector2Int displaySize = controller.DisplaySize;

#if UNITY_2022_2_OR_NEWER
            if (cameraData.xr.enabled)
            {
                // We force render scaling by modifying the camera's target texture descriptor, from which all of URP's render buffers are derived.
                // Doing this instead of using render scale ensures that the XR System allocates its output textures at full resolution, allowing for proper upscaling.
                if (_renderPipelineAsset != null)
                {
                    // Apply render scale from the URP asset settings
                    float renderScale = _renderPipelineAsset.renderScale;
                    bool disableRenderScale = Mathf.Abs(1.0f - renderScale) < 0.05f;
                    renderScale = disableRenderScale ? 1.0f : renderScale;
                    maxRenderSize.x = Mathf.Max(1, (int)(maxRenderSize.x * renderScale));
                    maxRenderSize.y = Mathf.Max(1, (int)(maxRenderSize.y * renderScale));
                }

                cameraData.cameraTargetDescriptor.width = maxRenderSize.x;
                cameraData.cameraTargetDescriptor.height = maxRenderSize.y;
                
                UpscalingHelpers.SetCameraPixelSize(ref cameraData, maxRenderSize);

                // Overriding the camera pixel size negates URP's global mip bias calculation, but fortunately we can override that too
                float mipBias = Math.Min((float)-Math.Log((float)displaySize.x / maxRenderSize.x, 2.0f), 0.0f);
                UpscalingHelpers.SetTaaMipBias(ref cameraData, mipBias);
            }
#endif

            if (_setupPass.Setup(controller, _upscalingRenderPass, _autoReactiveMaskPass))
            {
                renderer.EnqueuePass(_setupPass);
            }
            
            OpaqueCopyPass opaqueOnlySource = null;
            if (controller.EnableOpaqueOnlyCopy)
            {
                renderer.EnqueuePass(_opaqueCopyPass);
                opaqueOnlySource = _opaqueCopyPass;
            }

            AutoReactiveMaskPass autoReactiveSource = null;
            if (controller.autoGenerateReactiveMask && _autoReactiveMaskPass.Setup(controller, opaqueOnlySource))
            {
                renderer.EnqueuePass(_autoReactiveMaskPass);
                autoReactiveSource = _autoReactiveMaskPass;
            }

            if (controller.EnableCustomReactiveMask && _customReactiveMaskPass.Setup(controller.customReactiveMaskLayer))
            {
                renderer.EnqueuePass(_customReactiveMaskPass);
            }
            
            if (_upscalingRenderPass.Setup(_renderPipelineAsset, controller, _usingRenderGraph, _copyDepthMaterial, _upsampleDepthMaterial, opaqueOnlySource, autoReactiveSource))
            {
                renderer.EnqueuePass(_upscalingRenderPass);
            }

            if (_cleanupPass.Setup(maxRenderSize, displaySize))
            {
                renderer.EnqueuePass(_cleanupPass);
            }
        }

#if TND_URP_RENDERGRAPH
        // This method is called after URP sets up its own TAA jitter, and before RecordRenderGraph is called on all render passes.
        // That makes this the ideal place to set up jitter for upscaling and ensure it gets picked up correctly by every render pass.
        public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            if (!Application.isPlaying || !isActive || !_usingRenderGraph)
            {
                return;
            }

            if (!cameraData.camera.TryGetComponent<UpscalerController_URP>(out var controller) || !controller.enabled)
            {
                return;
            }
            
            Matrix4x4 jitterMatrix = _upscalingRenderPass.SetupJitter(controller, cameraData.camera.pixelWidth);
            UpscalingHelpers.SetCameraJitterMatrixRenderGraph(cameraData, jitterMatrix);
        }
#endif

        private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (!Application.isPlaying || !isActive)
            {
                return;
            }

            if (!camera.TryGetComponent<UpscalerController_URP>(out var controller) || !controller.enabled)
            {
                return;
            }

            // In non-XR scenarios we use regular URP render scale to change the camera's render resolution.
            // This is the most compatible option for proper cooperation with other third-party assets, which may not be aware of our upscaler assets.
            if (!controller.XREnabled && _renderPipelineAsset != null)
            {
                _renderPipelineAsset.renderScale = controller.RenderScale;
            }

#if !UNITY_2022_2_OR_NEWER
            // URP 13 and older do not have a separate camera jitter matrix for TAA yet,
            // so instead we modify the projection matrix on-the-fly as the camera is about to start rendering.
            _upscalingRenderPass.Setup(controller);
            Matrix4x4 jitterMatrix = _upscalingRenderPass.SetupJitter(controller, camera.pixelWidth);
            
            camera.ResetProjectionMatrix();
            Matrix4x4 projectionMatrix = camera.projectionMatrix;
            camera.nonJitteredProjectionMatrix = projectionMatrix;
            camera.projectionMatrix = jitterMatrix * projectionMatrix;
            camera.useJitteredProjectionMatrixForTransparentRendering = true;
#endif
        }

        private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (!Application.isPlaying || !isActive)
            {
                return;
            }

            // Reset rendering setup only when upscaling has been active on this camera
            if (!camera.TryGetComponent(out TNDUpscaler controller) || !controller.enabled)
            {
                return;
            }
            
            if (_renderPipelineAsset != null)
            {
                _renderPipelineAsset.renderScale = 1.0f;
            }

#if !UNITY_2022_2_OR_NEWER
            camera.ResetProjectionMatrix();
#endif
        }
    }
}
