using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace TND.Upscaling.Framework.HDRP
{
    [System.Serializable]
    public class AutoReactiveMaskPass : CustomPass
    {
        private RTHandle _reactiveMask;

        public Texture ReactiveMaskTexture => _reactiveMask?.rt;

        public OpaqueCopyPass OpaqueCopyPass { get; set; }
        public Material AutoReactiveMaterial { get; set; }
        public AutoReactiveSettings AutoReactiveSettings { get; set; }
        public GraphicsFormat ReactiveMaskFormat { get; set; } = GraphicsFormat.R8_UNorm;
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int OpaqueOnlyId = Shader.PropertyToID("_OpaqueOnly");
        private static readonly int ReactiveParamsId = Shader.PropertyToID("_ReactiveParams");
        private static readonly int ReactiveFlagsId = Shader.PropertyToID("_ReactiveFlags");

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Auto-Reactive Mask Pass";
            targetColorBuffer = TargetBuffer.None;
            targetDepthBuffer = TargetBuffer.None;

            CreateReactiveMaskTexture();
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (_reactiveMask?.rt != null && ReactiveMaskFormat != _reactiveMask.rt.graphicsFormat)
            {
                CreateReactiveMaskTexture();
            }

            CoreUtils.SetRenderTarget(ctx.cmd, _reactiveMask);
            
            if (AutoReactiveMaterial == null || OpaqueCopyPass?.OpaqueOnlyTexture == null)
            {
                CoreUtils.ClearRenderTarget(ctx.cmd, ClearFlag.Color, Color.clear);
                return;
            }
            
            if (TextureXR.useTexArray)
                ctx.cmd.EnableShaderKeyword(UpscalerContext.TexArraysKeyword);
            else
                ctx.cmd.DisableShaderKeyword(UpscalerContext.TexArraysKeyword);

            ctx.propertyBlock.SetTexture(MainTexId, ctx.cameraColorBuffer);
            ctx.propertyBlock.SetTexture(OpaqueOnlyId, OpaqueCopyPass.OpaqueOnlyTexture);
            ctx.propertyBlock.SetVector(ReactiveParamsId, new Vector3(AutoReactiveSettings.scale, AutoReactiveSettings.cutoffThreshold, AutoReactiveSettings.binaryValue));
            ctx.propertyBlock.SetInt(ReactiveFlagsId, (int)AutoReactiveSettings.flags);
            
            ctx.cmd.DrawProcedural(Matrix4x4.identity, AutoReactiveMaterial, 0, MeshTopology.Triangles, 3, TextureXR.slices, ctx.propertyBlock);
        }

        protected override void Cleanup()
        {
            if (_reactiveMask != null)
            {
                _reactiveMask.Release();
                _reactiveMask = null;
            }
        }

        private void CreateReactiveMaskTexture()
        {
            _reactiveMask?.Release();
            _reactiveMask = RTHandles.Alloc(Vector2.one, colorFormat: ReactiveMaskFormat, dimension: TextureXR.dimension, slices: TextureXR.slices, useDynamicScale: true, name: "AutoReactiveMask");
        }
    }
}
