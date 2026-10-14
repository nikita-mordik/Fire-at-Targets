#if TND_URP_RENDERGRAPH
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using TND.Common.URP;

namespace TND.Upscaling.Framework.URP
{
    public partial class UpscalingRenderPass
    {
        private static readonly int ScreenSizePropertyID = Shader.PropertyToID("_ScreenSize");
        private static readonly int ScaledScreenParamsPropertyID = Shader.PropertyToID("_ScaledScreenParams");
        
        private readonly BaseRenderFunc<PassData, UnsafeGraphContext> _executePassDelegate;

        private class PassData
        {
            public UniversalCameraData cameraData;
            public TextureHandle activeColorTexture;
            
            public TextureHandle colorBuffer;
            public TextureHandle depthBuffer;
            public TextureHandle motionVectorBuffer;
            public TextureHandle opaqueOnly;
            public TextureHandle autoReactiveMask;
            public TextureHandle outputColor;
            public TextureHandle outputDepth;

            public Vector2Int displaySize;
            public int viewCount;
            public bool upsampleDepth;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            GetOutputDescriptors(cameraData.cameraTargetDescriptor, _currentController.DisplaySize, out var colorDescriptor, out var depthDescriptor);
            UpscalingHelpers.SetupUpscaledColorHandles(cameraData.renderer, colorDescriptor);
            
            bool postProcessingEnabled = _currentController.injectionPoint < UpscalerInjectionPoint.AfterPostProcessing && cameraData.postProcessEnabled;
            ScriptableRenderPassInput ppInputs = UpscalingHelpers.GetRequiredPostProcessInputs(cameraData.renderer, postProcessingEnabled, renderPassEvent);
            bool upsampleDepth = (ppInputs & ScriptableRenderPassInput.Depth) != 0;

            TextureHandle outputColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorDescriptor, "_CameraUpscaledColor", false);
            TextureHandle outputDepth = upsampleDepth ? UniversalRenderer.CreateRenderGraphTexture(renderGraph, depthDescriptor, "_CameraUpsampledDepth", false) : TextureHandle.nullHandle;
            
            using (var builder = renderGraph.AddUnsafePass<PassData>(PassName, out var passData))
            {
                passData.cameraData = cameraData;
                passData.activeColorTexture = resourceData.activeColorTexture;
                
                passData.colorBuffer = resourceData.cameraColor;
                passData.depthBuffer = resourceData.cameraDepth;
                passData.motionVectorBuffer = resourceData.motionVectorColor;
                passData.opaqueOnly = _currentOpaqueOnlySource?.TextureHandle ?? TextureHandle.nullHandle;
                passData.autoReactiveMask = _currentAutoReactiveSource?.TextureHandle ?? TextureHandle.nullHandle;
                passData.outputColor = outputColor;
                passData.outputDepth = outputDepth;
                
                passData.displaySize = _currentController.DisplaySize;
                passData.viewCount = cameraData.xr.enabled ? cameraData.xr.viewCount : 1;
                passData.upsampleDepth = upsampleDepth;
                
                builder.UseTexture(passData.activeColorTexture);
                builder.UseTexture(passData.colorBuffer);
                builder.UseTexture(passData.depthBuffer);
                builder.UseTexture(passData.motionVectorBuffer);
                if (passData.opaqueOnly.IsValid())
                {
                    builder.UseTexture(passData.opaqueOnly);
                }
                if (passData.autoReactiveMask.IsValid())
                {
                    builder.UseTexture(passData.autoReactiveMask);
                }

                builder.UseTexture(outputColor, AccessFlags.Write);
                resourceData.cameraColor = outputColor;

                if (upsampleDepth)
                {
                    builder.UseTexture(outputDepth, AccessFlags.Write);
                    resourceData.cameraDepth = outputDepth;
                }

                builder.AllowPassCulling(false);
                builder.SetRenderFunc(_executePassDelegate);
            }

            if (postProcessingEnabled)
            {
                // Inform the post-processing passes of the new render resolution
                UpscalingHelpers.UpdatePostProcessDescriptors(cameraData.renderer, colorDescriptor);
            }
            
            UpdateCameraResolution(renderGraph, cameraData, colorDescriptor);
            
            if (upsampleDepth)
            {
                UpscalingHelpers.SetCameraDepthTexture(resourceData, outputDepth);
            }
        }

        private void ExecutePass(PassData passData, UnsafeGraphContext context)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            // Execute Upscaler
            for (int view = 0; view < passData.viewCount; ++view)
            {
                DispatchUpscaler(cmd, UpscalingHelpers.GetGPUProjectionMatrixNoJitter(passData.cameraData, passData.activeColorTexture, view), 
                    passData.cameraData.xr.multipassId * passData.viewCount + view,
                    new TextureRef(passData.colorBuffer),
                    new TextureRef(passData.depthBuffer),
                    new TextureRef(passData.motionVectorBuffer),
                    new TextureRef(passData.opaqueOnly),
                    new TextureRef(passData.autoReactiveMask),
                    new TextureRef(passData.outputColor));
            }

            // Prepare the Depth output for the next render pass
            if (passData.upsampleDepth)
            {
                UpsampleDepth(cmd, passData.cameraData.renderer, passData.displaySize, passData.cameraData.postProcessEnabled, passData.depthBuffer, passData.viewCount, passData.outputDepth);
            }
        }
        
        private class UpdateCameraResolutionPassData
        {
            public Vector2Int newCameraTargetSize;
        }
        
        // This is originally part of "UpdateCameraResolution" of the PostProcessPassRenderGraph internal function, so we had to move it
        private static void UpdateCameraResolution(RenderGraph renderGraph, UniversalCameraData cameraData, in RenderTextureDescriptor upscaledDesc)
        {
            cameraData.cameraTargetDescriptor.width = upscaledDesc.width;
            cameraData.cameraTargetDescriptor.height = upscaledDesc.height;

            // Update the shader constants to reflect the new camera resolution
            using (var builder = renderGraph.AddUnsafePass<UpdateCameraResolutionPassData>("[Upscaler] Update Camera Resolution", out var passData))
            {
                passData.newCameraTargetSize = new Vector2Int(upscaledDesc.width, upscaledDesc.height);

                // This pass only modifies shader constants so we need to set some special flags to ensure it isn't culled or optimized away
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (UpdateCameraResolutionPassData data, UnsafeGraphContext ctx) =>
                {
                    ctx.cmd.SetGlobalVector(ScaledScreenParamsPropertyID, new Vector4(data.newCameraTargetSize.x, data.newCameraTargetSize.y, 1.0f + 1.0f / data.newCameraTargetSize.x, 1.0f + 1.0f / data.newCameraTargetSize.y));
                    ctx.cmd.SetGlobalVector(ScreenSizePropertyID, new Vector4(data.newCameraTargetSize.x, data.newCameraTargetSize.y, 1.0f / data.newCameraTargetSize.x, 1.0f / data.newCameraTargetSize.y));
                });
            }
        }
    }
}
#endif  // TND_URP_RENDERGRAPH
