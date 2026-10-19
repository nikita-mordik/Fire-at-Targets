using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TND.Common.URP;

#if TND_URP_RENDERGRAPH
using UnityEngine.Rendering.RenderGraphModule;
#pragma warning disable 0672    // Disable obsolete warnings
#pragma warning disable 0618    // Disable obsolete warnings
#endif

namespace TND.Upscaling.Framework.URP
{
    public class OpaqueCopyPass : ScriptableRenderPass
    {
        private const string PassName = "[Upscaler] Opaque-Only Copy";

        private RTHandle _opaqueOnlyColor;
        public Texture Texture => _opaqueOnlyColor;

        public OpaqueCopyPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public void Dispose()
        {
            if (_opaqueOnlyColor != null)
            {
                _opaqueOnlyColor.Release();
                _opaqueOnlyColor = null;
            }
        }
        
        private RenderTextureDescriptor GetTextureDescriptor(in RenderTextureDescriptor cameraTargetDescriptor)
        {
            var descriptor = cameraTargetDescriptor;
            descriptor.depthStencilFormat = GraphicsFormat.None;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            return descriptor;
        }

#if TND_URP_RENDERGRAPH
        private TextureHandle _opaqueOnlyColorHandle;
        public TextureHandle TextureHandle => _opaqueOnlyColorHandle;
        
        private class PassData
        {
            public TextureHandle activeColorTexture;
            public TextureHandle opaqueOnlyColor;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddUnsafePass<PassData>(PassName, out var passData))
            {
                var cameraData = frameData.Get<UniversalCameraData>();
                var resourceData = frameData.Get<UniversalResourceData>();
                
                _opaqueOnlyColorHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, GetTextureDescriptor(cameraData.cameraTargetDescriptor), "_CameraOpaqueOnlyColor", false);

                passData.activeColorTexture = resourceData.activeColorTexture;
                passData.opaqueOnlyColor = _opaqueOnlyColorHandle;

                builder.UseTexture(passData.activeColorTexture);
                builder.UseTexture(passData.opaqueOnlyColor, AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc(static (PassData data, UnsafeGraphContext context) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                    cmd.CopyTexture(data.activeColorTexture, data.opaqueOnlyColor);
                });
            }
        }
#endif

#if TND_URP_COMPATIBILITY
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            CreateResources(renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            ReleaseResources();
        }

        private void CreateResources(in RenderTextureDescriptor cameraTargetDescriptor)
        {
            UpscalingHelpers.AllocateRTHandle(ref _opaqueOnlyColor, GetTextureDescriptor(cameraTargetDescriptor), FilterMode.Point, TextureWrapMode.Clamp, "_CameraOpaqueOnlyColor");
        }

        private void ReleaseResources()
        {
            UpscalingHelpers.ReleaseRTHandle(ref _opaqueOnlyColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(PassName);

            
#if UNITY_2022_1_OR_NEWER
            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, _opaqueOnlyColor);
#else
            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.Blit(cameraColorTarget, _opaqueOnlyColor);
#endif
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
#endif
    }
}
