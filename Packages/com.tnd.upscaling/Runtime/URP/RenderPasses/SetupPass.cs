using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if TND_URP_RENDERGRAPH
using UnityEngine.Rendering.RenderGraphModule;
#pragma warning disable 0672    // Disable obsolete warnings
#endif

namespace TND.Upscaling.Framework.URP
{
    public class SetupPass: ScriptableRenderPass
    {
        private const string PassName = "[Upscaler] Setup Pass";

        private UpscalerController_URP _currentController;
        private UpscalingRenderPass _upscalingRenderPass;
        private AutoReactiveMaskPass _autoReactiveMaskPass;
        
        public SetupPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRendering;
        }
        
        public bool Setup(UpscalerController_URP controller, UpscalingRenderPass upscalingRenderPass, AutoReactiveMaskPass autoReactiveMaskPass)
        {
            _currentController = controller;
            _upscalingRenderPass = upscalingRenderPass;
            _autoReactiveMaskPass = autoReactiveMaskPass;
            return _currentController != null;
        }
        
#if TND_URP_COMPATIBILITY
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            PatchUpscalingRenderPassEvent(100);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Nothing to do
        }
#endif

#if TND_URP_RENDERGRAPH
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            PatchUpscalingRenderPassEvent(0);
            
            if (_autoReactiveMaskPass != null)
            {
                // Change the reactive mask pass's render event so that it gets recorded after transparency drawing,
                // but after it has been sorted to be in front of all the other AfterRenderingTransparents passes.
                _autoReactiveMaskPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }
        }
#endif

        private void PatchUpscalingRenderPassEvent(int offset)
        {
            if (_upscalingRenderPass != null && _currentController.injectionPoint >= UpscalerInjectionPoint.AfterPostProcessing)
            {
                // URP only looks at passes with exactly AfterRenderingPostProcessing render pass event to decide whether post-processing should write directly to the backbuffer or not.
                // We want to force post-processing to write to a render texture from the camera color buffer system, which can be passed into the upscalers.
                // At the same time we want upscaling to always be sorted after any other after-postprocessing effects.
                // So we set the render pass event to +100 for sorting purposes, and make sure it's set to exactly AfterRenderingPostProcessing when URP decides the post-processing behavior.
                _upscalingRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing + offset;
            }
        }
    }
}
