using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TND.SGSR
{
    public class SGSRRenderFeature : ScriptableRendererFeature
    {
        [HideInInspector]
        internal bool isEnabled = false;

        private SGSR_URP m_upscaler;

        private SGSRRenderPass _renderPass;
        private SGSRBufferPass _bufferPass;

        private CameraData cameraData;

        public static bool usingRenderGraph = false;

        public override void Create()
        {
            name = "SGSRRenderFeature";

            SetupPasses();
        }

        public void OnSetReference(SGSR_URP _upscaler)
        {
            m_upscaler = _upscaler;
            SetupPasses();
        }

        private void SetupPasses()
        {
#if UNITY_2023_3_OR_NEWER
            var renderGraphSettings = GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>();
            usingRenderGraph = !renderGraphSettings.enableRenderCompatibilityMode;
#endif

            if (!usingRenderGraph)
            {
                _bufferPass = new SGSRBufferPass(m_upscaler);
            }

            _renderPass = new SGSRRenderPass(m_upscaler, usingRenderGraph);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if (!isEnabled || m_upscaler == null)
            {
                return;
            }

            cameraData = renderingData.cameraData;
            if (cameraData.camera != m_upscaler.m_mainCamera)
            {
                return;
            }
            if (!usingRenderGraph)
            {
                renderer.EnqueuePass(_bufferPass);
            }
            renderer.EnqueuePass(_renderPass);
        }
    }
}
