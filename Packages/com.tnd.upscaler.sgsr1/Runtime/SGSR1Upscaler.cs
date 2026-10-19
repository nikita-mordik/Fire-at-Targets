using UnityEngine;
using UnityEngine.Rendering;
using TND.Upscaling.Framework;

namespace TND.Upscaling.SGSR1
{
    public class SGSR1Upscaler: UpscalerBase<SGSR1UpscalerSettings>
    {
        public static readonly string DisplayName = "SGSR 1.1";

        public override bool RequiresRandomWriteOutput => false;

        private static readonly int BlitTextureID = Shader.PropertyToID("_BlitTexture");
        private static readonly int IntermediateTextureID = Shader.PropertyToID("_UpscalerOutput");
        private static readonly int ViewportInfoID = Shader.PropertyToID("ViewportInfo"); 
        private static readonly int EdgeSharpnessID = Shader.PropertyToID("EdgeSharpness");
        private static readonly int EdgeThresholdID = Shader.PropertyToID("EdgeThreshold");

        private UpscalerInitParams _initParams;
        private ShaderRef _sgsrBlitShader;
        private Material _sgsrBlitMaterial;
        private readonly MaterialPropertyBlock _sgsrBlitProperties = new();
        
        public SGSR1Upscaler(SGSR1UpscalerSettings settings) 
            : base(settings)
        {
        }

        public override bool Initialize(CommandBuffer commandBuffer, in UpscalerInitParams initParams)
        {
            _initParams = initParams;
            return UpscalerUtils.TryLoadMaterial("SGSR1/SGSR1_BlitShader", ref _sgsrBlitMaterial, ref _sgsrBlitShader);
        }

        public override void Destroy(CommandBuffer commandBuffer)
        {
            base.Destroy(commandBuffer);
            
            UpscalerUtils.UnloadMaterial(ref _sgsrBlitMaterial, ref _sgsrBlitShader);
        }

        public override void Dispatch(CommandBuffer commandBuffer, in UpscalerDispatchParams dispatchParams)
        {
            commandBuffer.BeginSample(DisplayName);

            if (dispatchParams.enableSharpening)
            {
                _initParams.GetMatchingTemporaryRT(commandBuffer, IntermediateTextureID, _initParams.upscaleSize, dispatchParams.outputColor.GraphicsFormat, false);
            }
            
            int depthSlice = dispatchParams.viewIndex;
            bool sizeMismatch = (dispatchParams.inputColor.Width != dispatchParams.renderSize.x || dispatchParams.inputColor.Height != dispatchParams.renderSize.y) 
                                && dispatchParams.inputColor.Width >= dispatchParams.renderSize.x && dispatchParams.inputColor.Height >= dispatchParams.renderSize.y;
            
            if (sizeMismatch)
            {
                // SGSR1 doesn't handle partially rendered viewports properly, so we copy the viewport to a correctly sized temporary texture
                _initParams.GetMatchingTemporaryRT(commandBuffer, BlitTextureID, dispatchParams.renderSize, dispatchParams.inputColor.GraphicsFormat, false);
                commandBuffer.CopyTexture(
                    dispatchParams.inputColor.GetRenderTargetIdentifier(), depthSlice, 0, 0, 0, dispatchParams.renderSize.x, dispatchParams.renderSize.y, 
                    BlitTextureID, depthSlice, 0, 0, 0);
            }
            else
            {
                commandBuffer.SetGlobalTexture(BlitTextureID, dispatchParams.inputColor.GetRenderTargetIdentifier(depthSlice));
            }
            
            _sgsrBlitProperties.SetVector(ViewportInfoID, new Vector4(1.0f / dispatchParams.renderSize.x, 1.0f / dispatchParams.renderSize.y, dispatchParams.renderSize.x, dispatchParams.renderSize.y));
            _sgsrBlitProperties.SetFloat(EdgeSharpnessID, Settings.edgeSharpness);
            _sgsrBlitProperties.SetFloat(EdgeThresholdID, Settings.edgeThreshold / 255.0f);
            commandBuffer.SetRenderTarget(dispatchParams.enableSharpening ? IntermediateTextureID : dispatchParams.outputColor.GetRenderTargetIdentifier(depthSlice), 0, CubemapFace.Unknown, depthSlice);
            commandBuffer.DrawProcedural(Matrix4x4.identity, _sgsrBlitMaterial, Settings.useEdgeDirection ? 1 : 0, MeshTopology.Triangles, 3, 1, _sgsrBlitProperties);

            if (sizeMismatch)
            {
                commandBuffer.ReleaseTemporaryRT(BlitTextureID);
            }

            if (dispatchParams.enableSharpening)
            {
                SharpenPass(commandBuffer, IntermediateTextureID, dispatchParams.outputColor.GetRenderTargetIdentifier(depthSlice), dispatchParams.sharpness);
                commandBuffer.ReleaseTemporaryRT(IntermediateTextureID);
            }
            
            commandBuffer.EndSample(DisplayName);
        }
    }
}
