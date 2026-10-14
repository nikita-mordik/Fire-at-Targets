#if TND_URP_RENDERGRAPH
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace TND.Upscaling.Framework.URP
{
    public partial class AutoReactiveMaskPass
    {
        private TextureHandle _autoReactiveMaskHandle;
        public TextureHandle TextureHandle => _autoReactiveMaskHandle;
        
        private class PassData
        {
            public TextureHandle activeColorTexture;
            public TextureHandle opaqueOnlyColor;
            public TextureHandle autoReactiveMask;

            public bool useTexArray;
            public int viewCount;
            public Material autoReactiveMaterial;
            public AutoReactiveSettings autoReactiveSettings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
            {
                var cameraData = frameData.Get<UniversalCameraData>();
                var resourceData = frameData.Get<UniversalResourceData>();

                _autoReactiveMaskHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, GetTextureDescriptor(cameraData.cameraTargetDescriptor), "_AutoReactiveMask", false);

                passData.activeColorTexture = resourceData.activeColorTexture;
                passData.opaqueOnlyColor = _opaqueOnlySource.TextureHandle;
                passData.useTexArray = _currentController.IsSinglePassXR;
                passData.viewCount = cameraData.xr.enabled ? cameraData.xr.viewCount : 1;
                passData.autoReactiveMaterial = _currentController.GetUpscalerContext().AutoReactiveMaterial;
                passData.autoReactiveSettings = _currentController.autoReactiveSettings;

                builder.UseTexture(passData.activeColorTexture);
                builder.UseTexture(passData.opaqueOnlyColor);
                
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderAttachment(_autoReactiveMaskHandle, 0);
                
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    if (data.autoReactiveMaterial == null || !data.opaqueOnlyColor.IsValid())
                    {
                        context.cmd.ClearRenderTarget(false, true, Color.clear);
                        return;
                    }
                    
                    if (data.useTexArray)
                        context.cmd.EnableShaderKeyword(UpscalerContext.TexArraysKeyword);
                    else
                        context.cmd.DisableShaderKeyword(UpscalerContext.TexArraysKeyword);
                    
                    var mbp = context.renderGraphPool.GetTempMaterialPropertyBlock();
                    mbp.SetTexture(MainTexId, data.activeColorTexture);
                    mbp.SetTexture(OpaqueOnlyId, data.opaqueOnlyColor);
                    mbp.SetVector(ReactiveParamsId, new Vector3(data.autoReactiveSettings.scale, data.autoReactiveSettings.cutoffThreshold, data.autoReactiveSettings.binaryValue));
                    mbp.SetInt(ReactiveFlagsId, (int)data.autoReactiveSettings.flags);
                    
                    context.cmd.DrawProcedural(Matrix4x4.identity, data.autoReactiveMaterial, 0, MeshTopology.Triangles, 3, data.viewCount, mbp);
                });
            }

            // Reset event order so that this pass gets sorted before other AfterRenderingTransparents passes on the next frame
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents - 1;
        }
    }
}
#endif  // TND_URP_RENDERGRAPH
