using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_2023_3_OR_NEWER
using System;
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace TND.SGSR
{
    public class SGSRRenderPass : ScriptableRenderPass
    {
        private SGSR_URP m_upscaler;
        private Material _blitMaterial;
        private const string blitPass = "[SGSR] Upscaler";

        public static bool usingRenderGraph = false;

        //Legacy
        private RenderTargetIdentifier _cameraTarget;
        private Vector4 _scaleBias;

        private readonly string _blitShader = "Hidden/SGSR_BlitShader_URP";

        public SGSRRenderPass(SGSR_URP _upscaler, bool usingRenderGraph)
        {
            m_upscaler = _upscaler;

            renderPassEvent = usingRenderGraph ? RenderPassEvent.AfterRenderingPostProcessing : RenderPassEvent.AfterRendering + 2;

            _blitMaterial = CoreUtils.CreateEngineMaterial(_blitShader);
            _cameraTarget = BuiltinRenderTextureType.CameraTarget;
            _scaleBias = SystemInfo.graphicsUVStartsAtTop ? new Vector4(1, -1, 0, 1) : Vector4.one;
        }

        #region Unity 6

#if UNITY_2023_3_OR_NEWER

        private class PassData
        {
            public TextureHandle Source;
            public TextureHandle Destination;
            public Material Material;
            public Rect PixelRect;
            public float EdgeSharpness;
            public Vector4 ViewportInfo;
        }

        private const string _upscaledTextureName = "_SGSR_UpscaledTexture";

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Setting up the render pass in RenderGraph
            using (var builder = renderGraph.AddUnsafePass<PassData>(blitPass, out var passData))
            {
                var cameraData = frameData.Get<UniversalCameraData>();
                var resourceData = frameData.Get<UniversalResourceData>();


                RenderTextureDescriptor upscaledDesc = cameraData.cameraTargetDescriptor;
                upscaledDesc.depthBufferBits = 0;
                upscaledDesc.width = m_upscaler.displayWidth;
                upscaledDesc.height = m_upscaler.displayHeight;

                TextureHandle upscaled = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    upscaledDesc,
                    _upscaledTextureName,
                    false
                );

                passData.Material = _blitMaterial;
                passData.Source = resourceData.activeColorTexture;
                passData.Destination = upscaled;
                passData.PixelRect = cameraData.camera.pixelRect;
                passData.EdgeSharpness = m_upscaler.edgeSharpness;
                passData.ViewportInfo = m_upscaler.viewportInfo;

                builder.UseTexture(passData.Source);
                builder.UseTexture(passData.Destination, AccessFlags.Write);

                builder.AllowPassCulling(false);

                resourceData.cameraColor = upscaled;
                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        private void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            CommandBuffer unsafeCmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            unsafeCmd.SetGlobalFloat(SGSR_UTILS.idEdgeSharpness, data.EdgeSharpness);
            unsafeCmd.SetGlobalVector(SGSR_UTILS.idViewportInfo, data.ViewportInfo);

            unsafeCmd.SetRenderTarget(data.Destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            unsafeCmd.SetViewport(data.PixelRect);

            Blitter.BlitTexture(unsafeCmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
        }
#endif

        #endregion


        #region Unity Legacy
#if UNITY_6000_0_OR_NEWER
        [Obsolete]
#endif
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer _cmd = CommandBufferPool.Get(blitPass);

            _cmd.SetGlobalFloat(SGSR_UTILS.idEdgeSharpness, m_upscaler.edgeSharpness);
            _cmd.SetGlobalVector(SGSR_UTILS.idViewportInfo, m_upscaler.viewportInfo);

            _cmd.SetRenderTarget(_cameraTarget);
            _cmd.SetViewport(renderingData.cameraData.camera.pixelRect);

            if (renderingData.cameraData.camera.targetTexture != null)
            {
                _scaleBias = Vector2.one;
            }

#if UNITY_2022_1_OR_NEWER
            Blitter.BlitTexture(_cmd, m_upscaler.colorBuffer, _scaleBias, _blitMaterial, 0);
#else
            _cmd.SetGlobalTexture(SGSR_UTILS.idBlitTexture, m_upscaler.colorBuffer);
            _cmd.SetGlobalVector(SGSR_UTILS.idScaleBias, _scaleBias);

            _cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _blitMaterial);
#endif

            context.ExecuteCommandBuffer(_cmd);
            CommandBufferPool.Release(_cmd);
        }
    }


    public class SGSRBufferPass : ScriptableRenderPass
    {
        private SGSR_URP m_upscaler;

#if !UNITY_2021_2_OR_NEWER//Unity Legacy
        private readonly int AfterPostProcessTexture = Shader.PropertyToID("_AfterPostProcessTexture");
#endif

        public SGSRBufferPass(SGSR_URP _upscaler)
        {
#if !UNITY_2021_2_OR_NEWER //Unity Legacy
            renderPassEvent = RenderPassEvent.AfterRendering;
#else
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
#endif

            m_upscaler = _upscaler;
        }

#if UNITY_6000_0_OR_NEWER
        [Obsolete]
#endif
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_2022_1_OR_NEWER
            m_upscaler.colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
#elif UNITY_2021_2_OR_NEWER
            m_upscaler.colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;
#else //Unity Legacy
            if (Shader.GetGlobalTexture(AfterPostProcessTexture) != null) {
                m_upscaler.colorBuffer = Shader.GetGlobalTexture(AfterPostProcessTexture);
            }
#endif
        }
        #endregion

    }
}
