using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TND.Common.URP;

#if TND_URP_RENDERGRAPH
#pragma warning disable 0672    // Disable obsolete warnings
#pragma warning disable 0618    // Disable obsolete warnings
#endif

namespace TND.Upscaling.Framework.URP
{
    public partial class UpscalingRenderPass : CameraJitterPass
    {
        private const string PassName = "[Upscaler] Upscaling Pass";
        
#if (UNITY_SWITCH || UNITY_ANDROID) && !UNITY_EDITOR
        private const GraphicsFormat DepthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
#else
        private const GraphicsFormat DepthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
#endif

        private static readonly int DepthTexturePropertyID = Shader.PropertyToID("_CameraDepthTexture");
        private static readonly int InputDepthPropertyID = Shader.PropertyToID("_InputDepthTexture");
        private static readonly int MotionTexturePropertyID = Shader.PropertyToID("_MotionVectorTexture");
        private static readonly int BlitScaleBiasID = Shader.PropertyToID("_BlitScaleBias");
        private static readonly int ViewIndexID = Shader.PropertyToID("_ViewIndex");

        private UniversalRenderPipelineAsset _currentRenderPipeline;
        private OpaqueCopyPass _currentOpaqueOnlySource;
        private AutoReactiveMaskPass _currentAutoReactiveSource;
        private IntPtr _currentCameraDataPtr;

        private Material _copyDepthMaterial;
        private Material _upsampleDepthMaterial;
        private readonly MaterialPropertyBlock _copyDepthProperties = new();
        private readonly MaterialPropertyBlock _upsampleDepthProperties = new();

        private RTHandle _upscalerOutput;
        private RTHandle _upsampledDepth;

        private readonly List<RTHandle> _tempTextures = new();
        private readonly TextureRef.BlitterDelegate _inputColorBlitter;
        private readonly TextureRef.BlitterDelegate _inputDepthBlitter;
        private readonly TextureRef.BlitterDelegate _inputMotionBlitter;

        public UpscalingRenderPass()
        {
#if TND_URP_RENDERGRAPH
            _executePassDelegate = ExecutePass;
#endif

            _inputColorBlitter = BlitColor;
            _inputDepthBlitter = BlitDepth;
            _inputMotionBlitter = BlitMotion;
            
            ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Motion);
        }

        /// <summary>
        /// Collect all information required for a single execution of this pass and validate
        /// whether everything is set up correctly to allow the pass to execute.
        /// </summary>
        public bool Setup(UniversalRenderPipelineAsset renderPipeline, UpscalerController_URP controller, bool usingRenderGraph,
            Material copyDepthMaterial, Material upsampleDepthMaterial, 
            OpaqueCopyPass opaqueOnlySource, AutoReactiveMaskPass autoReactiveMaskPass)
        {
            if (!base.Setup(controller))
                return false;

            switch (controller.injectionPoint)
            {
                case UpscalerInjectionPoint.BeforePostProcessing:
                    // Ensure upscaling happens before all built-in and any custom post-processing passes
                    renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing - 10;
                    break;
                case UpscalerInjectionPoint.AfterPostProcessing:
                    // Ensure upscaling happens after any custom post-processing passes, but before the final blit pass
                    renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing + (usingRenderGraph ? 100 : 0);
                    break;
            }

            _currentRenderPipeline = renderPipeline;
            _copyDepthMaterial = copyDepthMaterial;
            _upsampleDepthMaterial = upsampleDepthMaterial;
            _currentOpaqueOnlySource = opaqueOnlySource;
            _currentAutoReactiveSource = autoReactiveMaskPass;
            return true;
        }

        public void Dispose()
        {
            if (_upscalerOutput != null)
            {
                _upscalerOutput.Release();
                _upscalerOutput = null;
            }

            if (_upsampledDepth != null)
            {
                _upsampledDepth.Release();
                _upsampledDepth = null;
            }
        }
        
        private void GetOutputDescriptors(in RenderTextureDescriptor cameraTargetDescriptor, in Vector2Int displaySize, out RenderTextureDescriptor outColorDescriptor, out RenderTextureDescriptor outDepthDescriptor)
        {
            outColorDescriptor = cameraTargetDescriptor;
            outColorDescriptor.width = displaySize.x;
            outColorDescriptor.height = displaySize.y;
            outColorDescriptor.depthStencilFormat = GraphicsFormat.None;
            outColorDescriptor.useMipMap = false;
            outColorDescriptor.autoGenerateMips = false;
            outColorDescriptor.bindMS = false;
            outColorDescriptor.enableRandomWrite = _currentController.ActiveUpscaler?.RequiresRandomWriteOutput ?? SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3;
            outColorDescriptor.msaaSamples = 1;
            
            outDepthDescriptor = outColorDescriptor;
            outDepthDescriptor.graphicsFormat = GraphicsFormat.None;
            outDepthDescriptor.depthStencilFormat = cameraTargetDescriptor.depthStencilFormat;
            outDepthDescriptor.enableRandomWrite = false;
        }
        
#if TND_URP_COMPATIBILITY
        /// <summary>
        /// OnCameraSetup gets called very early on in the process of rendering a camera.
        /// This can be used to make significant changes to the camera's parameters that affect all render passes.
        /// </summary>
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Apply jitter
            base.OnCameraSetup(cmd, ref renderingData);

            CreateResources(renderingData.cameraData.cameraTargetDescriptor, _currentController.DisplaySize);
            
#if UNITY_2022_2_OR_NEWER
            _currentCameraDataPtr = UpscalingHelpers.GetPtr(ref renderingData.cameraData);
#endif
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
#if UNITY_2022_2_OR_NEWER
            Vector2Int displaySize = _currentController.DisplaySize;
            if (_currentCameraDataPtr != IntPtr.Zero)
            {
                // Modify the camera target descriptor used by post-upscaling passes to allocate resources, so that they operate at the correct display resolution. 
                // This is all rather hacky with pointers and unsafe code, but we have no other way to modify the *original* camera target descriptor at this point in the pipeline.
                ref var cameraData = ref UpscalingHelpers.AsRef<CameraData>(_currentCameraDataPtr);
                cameraData.cameraTargetDescriptor.width = displaySize.x;
                cameraData.cameraTargetDescriptor.height = displaySize.y;
            }
#endif
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            ReleaseResources();
        }

        /// <summary>
        /// Create output textures for the upscaler to write to
        /// </summary>
        private void CreateResources(RenderTextureDescriptor cameraTargetDescriptor, in Vector2Int displaySize)
        {
            GetOutputDescriptors(cameraTargetDescriptor, displaySize, out var colorDescriptor, out var depthDescriptor);
            
            UpscalingHelpers.AllocateRTHandle(ref _upscalerOutput, colorDescriptor, FilterMode.Point, TextureWrapMode.Clamp, "_CameraUpscaledColor");
            UpscalingHelpers.AllocateRTHandle(ref _upsampledDepth, depthDescriptor, FilterMode.Point, TextureWrapMode.Clamp, "_CameraUpsampledDepth");
        }

        private void ReleaseResources()
        {
            UpscalingHelpers.ReleaseRTHandle(ref _upscalerOutput);
            UpscalingHelpers.ReleaseRTHandle(ref _upsampledDepth);

            for (int i = 0; i < _tempTextures.Count; ++i)
            {
                RTHandle tempTexture = _tempTextures[i];
                UpscalingHelpers.ReleaseRTHandle(ref tempTexture);
            }
            
            _tempTextures.Clear();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(PassName);
            ref var cameraData = ref renderingData.cameraData;

            Vector2Int displaySize = _currentController.DisplaySize;
            var postUpscaleDescriptor = cameraData.cameraTargetDescriptor;
            postUpscaleDescriptor.width = displaySize.x;
            postUpscaleDescriptor.height = displaySize.y;
            
#if UNITY_2022_2_OR_NEWER
            // Try to grab the motion vector texture directly from the scriptable renderer, but fall back to grabbing a named global texture for older URP versions.
            // The latter may not always result in the correct texture for the current camera, which is why we prefer to grab it directly off the renderer.
            Texture motionVectorTexture = UpscalingHelpers.GetMotionVectorTexture(cameraData.renderer) ?? Shader.GetGlobalTexture(MotionTexturePropertyID);
            RTHandle inputDepth = cameraData.renderer.cameraDepthTargetHandle;

            int viewCount = cameraData.xr.enabled ? cameraData.xr.viewCount : 1;
            for (int view = 0; view < viewCount; ++view)
            {
                DispatchUpscaler(cmd, cameraData.GetGPUProjectionMatrixNoJitter(view), cameraData.xr.multipassId * viewCount + view,
                    new TextureRef(cameraData.renderer.cameraColorTargetHandle),
                    new TextureRef(cameraData.renderer.cameraDepthTargetHandle),
                    new TextureRef(motionVectorTexture),
                    new TextureRef(_currentOpaqueOnlySource?.Texture),
                    new TextureRef(_currentAutoReactiveSource?.Texture),
                    new TextureRef(_upscalerOutput));
            }

            // Reconfigure the color buffer system and obtain the target texture for upscaling
            if (!UpscalingHelpers.PrepareUpscalerOutput(cmd, cameraData.renderer, postUpscaleDescriptor, out var upscalerTarget))
            {
                CommandBufferPool.Release(cmd);
                return;
            }
            
            // Inject the upscaler's output into the color buffer system
            Blitter.BlitCameraTexture(cmd, _upscalerOutput, upscalerTarget);

            if (!cameraData.xr.enabled)
            {
                _currentRenderPipeline.renderScale = 1.0f;
            }
#else
            RenderTargetIdentifier inputDepth = cameraData.renderer.cameraDepthTarget;

            var colorDescriptor = cameraData.cameraTargetDescriptor;
            colorDescriptor.depthStencilFormat = GraphicsFormat.None;
            colorDescriptor.useMipMap = false;
            colorDescriptor.autoGenerateMips = false;
            colorDescriptor.bindMS = false;

            var depthDescriptor = colorDescriptor;
            depthDescriptor.graphicsFormat = GraphicsFormat.None;
            depthDescriptor.depthStencilFormat = cameraData.cameraTargetDescriptor.depthStencilFormat;

            var motionDescriptor = colorDescriptor;
            motionDescriptor.graphicsFormat = GraphicsFormat.R16G16_SFloat;
            motionDescriptor.depthStencilFormat = GraphicsFormat.None;

            int viewCount = cameraData.GetXRViewCount();
            for (int view = 0; view < viewCount; ++view)
            {
                // Because we inject jitter into the camera's projection matrix for Unity 2021.x, we need to de-jitter it here to get the non-jittered version back
                Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(view);
                Matrix4x4 nonJitteredProjMatrix = GL.GetGPUProjectionMatrix(_inverseJitterMatrix * projectionMatrix, cameraData.IsCameraProjectionMatrixFlipped());
                
                DispatchUpscaler(cmd, nonJitteredProjMatrix, cameraData.GetXRMultiPassId() * viewCount + view,
                    new TextureRef(cameraData.renderer.cameraColorTarget, colorDescriptor, _inputColorBlitter),
                    new TextureRef(cameraData.renderer.cameraDepthTarget, depthDescriptor, _inputDepthBlitter),
                    new TextureRef(MotionTexturePropertyID, motionDescriptor, _inputMotionBlitter),
                    new TextureRef(_currentOpaqueOnlySource?.Texture),
                    new TextureRef(_currentAutoReactiveSource?.Texture),
                    new TextureRef(_upscalerOutput.rt));
            }

            // For Unity 2021.x this has to happen after the upscaler dispatch, otherwise the camera color target won't be correct as input for the upscaler
            if (!UpscalingHelpers.PrepareUpscalerOutput(cmd, cameraData.renderer, postUpscaleDescriptor, out var upscalerTarget))
            {
                CommandBufferPool.Release(cmd);
                return;
            }
            
            // Inject the upscaler's output into the color buffer system
            // Always perform the extra post-upscale blit on 2021.x, as the camera target can only be referred to via an indirect RenderTargetIdentifier
            cmd.Blit(_upscalerOutput.rt, upscalerTarget.Identifier());
            
            _currentRenderPipeline.renderScale = 1.0f;
#endif

            // Reset render size to full display resolution for subsequent passes
            cameraData.renderScale = 1.0f;
            cameraData.cameraTargetDescriptor.width = displaySize.x;
            cameraData.cameraTargetDescriptor.height = displaySize.y;

            // Prepare the upscaler output for the next render pass
            UpscalingHelpers.InjectUpscalerOutput(cmd, cameraData.renderer);
            UpscalingHelpers.UpdatePerCameraShaderVariables(cmd, ref cameraData);

            // Inform the post-processing passes of the new render resolution
            bool postProcessingEnabled = _currentController.injectionPoint < UpscalerInjectionPoint.AfterPostProcessing && renderingData.postProcessingEnabled;
            if (postProcessingEnabled)
            {
                UpscalingHelpers.UpdatePostProcessDescriptors(cameraData.renderer, cameraData.cameraTargetDescriptor);
            }
            
            // Prepare the Depth output for the next render pass
            UpsampleDepth(cmd, cameraData.renderer, displaySize, postProcessingEnabled, inputDepth, viewCount, _upsampledDepth);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
#endif

        private void DispatchUpscaler(CommandBuffer cmd, in Matrix4x4 nonJitteredProjMatrix, int viewIndex, 
            in TextureRef colorBuffer, in TextureRef depthBuffer, in TextureRef motionVectorBuffer,
            in TextureRef opaqueOnly, in TextureRef autoReactiveMask,
            in TextureRef output)
        {
            var upscalerContext = _currentController.GetUpscalerContext(viewIndex);
            if (upscalerContext == null)
                return;

            RTHandle tempColor = null, tempDepth = null, tempMotion = null, tempOpaque = null, tempAutoReactive = null, tempOutput = null;

            bool copyView = _currentController.IsSinglePassXR && viewIndex > 0;
            if (copyView)
            {
                // This feels like a bit of a cop-out, but requiring upscalers to properly support texture array slices higher than 0 adds a ton of complexity.
                // In many instances (particularly for native plugin upscalers) this would result in the upscaler copying the input slice to a temp texture to more easily deal with it.
                // So instead of burdening the upscalers with that complexity, we already copy non-zero texture array slices to temp textures here and present those to the upscaler.
                RenderTextureDescriptor colorDesc = new(colorBuffer.Width, colorBuffer.Height, colorBuffer.GraphicsFormat, GraphicsFormat.None);
                RenderTextureDescriptor depthDesc = new(depthBuffer.Width, depthBuffer.Height, GraphicsFormat.None, DepthStencilFormat);
                RenderTextureDescriptor motionDesc = new(motionVectorBuffer.Width, motionVectorBuffer.Height, motionVectorBuffer.GraphicsFormat, GraphicsFormat.None);
                RenderTextureDescriptor outputDesc = new(output.Width, output.Height, output.GraphicsFormat, GraphicsFormat.None)
                {
                    enableRandomWrite = _currentController.ActiveUpscaler?.RequiresRandomWriteOutput ?? false,
                };

                tempColor = AllocateTempTexture(colorDesc, "_TempColor");
                tempDepth = AllocateTempTexture(depthDesc, "_TempDepth");
                tempMotion = AllocateTempTexture(motionDesc, "_TempMotion");
                tempOutput = AllocateTempTexture(outputDesc, "_TempOutput");
                
                cmd.CopyTexture(colorBuffer.GetRenderTargetIdentifier(), viewIndex, tempColor, 0);
                cmd.CopyTexture(depthBuffer.GetRenderTargetIdentifier(), viewIndex, tempDepth, 0);
                cmd.CopyTexture(motionVectorBuffer.GetRenderTargetIdentifier(), viewIndex, tempMotion, 0);

                Texture inputOpaque = opaqueOnly.GetTexture(cmd);
                if (inputOpaque != null)
                {
                    tempOpaque = AllocateTempTexture(colorDesc, "_TempOpaque");
                    cmd.CopyTexture(inputOpaque, viewIndex, tempOpaque, 0);
                }

                Texture inputAutoReactive = autoReactiveMask.GetTexture(cmd);
                if (inputAutoReactive != null)
                {
                    RenderTextureDescriptor reactiveDesc = new(autoReactiveMask.Width, autoReactiveMask.Height, autoReactiveMask.GraphicsFormat, GraphicsFormat.None);
                    tempAutoReactive = AllocateTempTexture(reactiveDesc, "_TempAutoReactive");
                    cmd.CopyTexture(inputAutoReactive, viewIndex, tempAutoReactive, 0);
                }
            }
            
            // Gather inputs
            var scaledRenderSize = _currentController.ScaledRenderSize;
            var dispatchParams = new UpscalerDispatchParams
            {
                nonJitteredProjectionMatrix = nonJitteredProjMatrix,
                viewIndex = viewIndex,
                inputColor = copyView ? new TextureRef(tempColor) : colorBuffer,
                inputDepth = copyView ? new TextureRef(tempDepth) : depthBuffer,
                inputMotionVectors = copyView ? new TextureRef(tempMotion) : motionVectorBuffer,
                inputExposure = TextureRef.Null,
                inputReactiveMask = copyView ? new TextureRef(tempAutoReactive) : autoReactiveMask,
                inputOpaqueOnly = copyView ? new TextureRef(tempOpaque) : opaqueOnly,
                outputColor = copyView ? new TextureRef(tempOutput) : output,
                renderSize = scaledRenderSize,
                motionVectorScale = -scaledRenderSize,
                jitterOffset = _currentJitterOffset,
                preExposure = 1.0f,
                enableSharpening = _currentController.enableSharpening,
                sharpness = _currentController.sharpness,
                resetHistory = _currentController.ResetHistory,
            };
            
            upscalerContext.Execute(cmd, ref dispatchParams);

            if (copyView)
            {
                // Copy the upscaler's output to the correct output texture array slice
                cmd.CopyTexture(tempOutput, 0, output.GetRenderTargetIdentifier(), viewIndex);
            }
        }
        
        private void UpsampleDepth(CommandBuffer cmd, ScriptableRenderer renderer, in Vector2Int displaySize, bool postProcessingEnabled, RenderTargetIdentifier inputDepth, int viewCount, Texture outputDepth)
        {
            ScriptableRenderPassInput ppInputs = UpscalingHelpers.GetRequiredPostProcessInputs(renderer, postProcessingEnabled, renderPassEvent);
            if ((ppInputs & ScriptableRenderPassInput.Depth) != 0)
            {
                UpsampleDepth(cmd, displaySize, inputDepth, viewCount, outputDepth);
            }
        }

        private void UpsampleDepth(CommandBuffer cmd, in Vector2Int displaySize, RenderTargetIdentifier inputDepth, int viewCount, Texture outputDepth)
        {
            var renderSize = _currentController.ScaledRenderSize;
            float jitterBiasX = _currentJitterOffset.x / renderSize.x;
            float jitterBiasY = _currentJitterOffset.y / renderSize.y;

            // Dejitter and nearest-neighbor upsample the depth buffer to use as input for post-processing
            cmd.SetGlobalTexture(InputDepthPropertyID, inputDepth, RenderTextureSubElement.Depth);
            _upsampleDepthProperties.SetVector(BlitScaleBiasID, new Vector4(1.0f / displaySize.x, 1.0f / displaySize.y, jitterBiasX, jitterBiasY));
            
            // Unity stereo instancing for this shader is weirdly broken in Android Vulkan VR on 2022.3, so we just run the shader separately for each eye.
            for (int viewIndex = 0; viewIndex < viewCount; ++viewIndex)
            {
                cmd.SetRenderTarget(new RenderTargetIdentifier(outputDepth, 0, CubemapFace.Unknown, viewIndex), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                _upsampleDepthProperties.SetInteger(ViewIndexID, viewIndex);
                cmd.DrawProcedural(Matrix4x4.identity, _upsampleDepthMaterial, 0, MeshTopology.Triangles, 3, 1, _upsampleDepthProperties);
            }
            
            // Bind the upsampled depth buffer to the global camera depth texture
            cmd.SetGlobalTexture(DepthTexturePropertyID, outputDepth);
        }

        private Texture BlitColor(CommandBuffer cmd, in RenderTextureDescriptor desc, in RenderTargetIdentifier source)
        {
            RTHandle tempColor = AllocateTempTexture(desc, "_TempColor");
            cmd.Blit(source, tempColor);
            cmd.SetRenderTarget(BuiltinRenderTextureType.None);
            return tempColor;
        }

        private Texture BlitDepth(CommandBuffer cmd, in RenderTextureDescriptor desc, in RenderTargetIdentifier source)
        {
            RTHandle tempDepth = AllocateTempTexture(desc, "_TempDepth");
            
            CoreUtils.SetRenderTarget(cmd, tempDepth);
            _copyDepthMaterial.EnableKeyword("_USE_DRAW_PROCEDURAL");
            _copyDepthProperties.SetVector(BlitScaleBiasID, new Vector4(1, 1, 0, 0));
            cmd.DrawProcedural(Matrix4x4.identity, _copyDepthMaterial, 0, MeshTopology.Quads, 4, 1, _copyDepthProperties);
            cmd.SetRenderTarget(BuiltinRenderTextureType.None);
            
            return tempDepth;
        }

        private Texture BlitMotion(CommandBuffer cmd, in RenderTextureDescriptor desc, in RenderTargetIdentifier source)
        {
            RTHandle tempMotion = AllocateTempTexture(desc, "_TempMotion");
            cmd.Blit(source, tempMotion);
            cmd.SetRenderTarget(BuiltinRenderTextureType.None);
            return tempMotion;
        }

        private RTHandle AllocateTempTexture(in RenderTextureDescriptor desc, string name)
        {
            RTHandle tempTexture = null;
            UpscalingHelpers.AllocateRTHandle(ref tempTexture, desc, FilterMode.Point, TextureWrapMode.Clamp, name);
            _tempTextures.Add(tempTexture);
            return tempTexture;
        }
    }
}
