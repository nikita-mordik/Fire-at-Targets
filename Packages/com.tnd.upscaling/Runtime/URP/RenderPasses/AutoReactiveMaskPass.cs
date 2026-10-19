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
    public partial class AutoReactiveMaskPass: ScriptableRenderPass
    {
        private const string PassName = "[Upscaler] Auto Reactive Mask Pass";
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int OpaqueOnlyId = Shader.PropertyToID("_OpaqueOnly");
        private static readonly int ReactiveParamsId = Shader.PropertyToID("_ReactiveParams");
        private static readonly int ReactiveFlagsId = Shader.PropertyToID("_ReactiveFlags");

        private RTHandle _autoReactiveMask;
        public Texture Texture => _autoReactiveMask;

        private UpscalerController_URP _currentController;
        private OpaqueCopyPass _opaqueOnlySource;
        
        public AutoReactiveMaskPass()
        {
            // Ensure that this pass executes right before any effects that are applied after transparencies (for example: distance fog)
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents - 1;
            
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public bool Setup(UpscalerController_URP controller, OpaqueCopyPass opaqueOnlySource)
        {
            _currentController = controller;
            _opaqueOnlySource = opaqueOnlySource;
            return _currentController != null && _opaqueOnlySource != null;
        }
        
        public void Dispose()
        {
            if (_autoReactiveMask != null)
            {
                _autoReactiveMask.Release();
                _autoReactiveMask = null;
            }
        }
        
        private RenderTextureDescriptor GetTextureDescriptor(in RenderTextureDescriptor cameraTargetDescriptor)
        {
            var autoReactiveDescriptor = cameraTargetDescriptor;
            autoReactiveDescriptor.graphicsFormat = _currentController.ActiveUpscalerPlugin?.ReactiveMaskFormat ?? GraphicsFormat.R8_UNorm;
            autoReactiveDescriptor.depthStencilFormat = GraphicsFormat.None;
            autoReactiveDescriptor.useMipMap = false;
            autoReactiveDescriptor.autoGenerateMips = false;
            autoReactiveDescriptor.bindMS = false;
            return autoReactiveDescriptor;
        }
        
#if TND_URP_COMPATIBILITY
        private readonly MaterialPropertyBlock _propertyBlock = new();
        
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
            UpscalingHelpers.AllocateRTHandle(ref _autoReactiveMask, GetTextureDescriptor(cameraTargetDescriptor), FilterMode.Point, TextureWrapMode.Clamp, "_AutoReactiveMask");
        }

        private void ReleaseResources()
        {
            UpscalingHelpers.ReleaseRTHandle(ref _autoReactiveMask);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(PassName);
            ref var cameraData = ref renderingData.cameraData;

#if UNITY_2022_2_OR_NEWER
            var cameraColorTarget = cameraData.renderer.cameraColorTargetHandle;
            int viewCount = cameraData.xr.enabled ? cameraData.xr.viewCount : 1;
#else
            var cameraColorTarget = cameraData.renderer.cameraColorTarget;
            int viewCount = cameraData.GetXRViewCount();
#endif
            
            CoreUtils.SetRenderTarget(cmd, _autoReactiveMask);
            
            Material autoReactiveMaterial = _currentController.GetUpscalerContext().AutoReactiveMaterial;
            if (autoReactiveMaterial != null && _opaqueOnlySource.Texture != null)
            {
                if (_currentController.IsSinglePassXR)
                    cmd.EnableShaderKeyword(UpscalerContext.TexArraysKeyword);
                else
                    cmd.DisableShaderKeyword(UpscalerContext.TexArraysKeyword);
                
                var autoReactiveSettings = _currentController.autoReactiveSettings;
#if UNITY_2022_2_OR_NEWER
                _propertyBlock.SetTexture(MainTexId, cameraColorTarget);
#else
                cmd.SetGlobalTexture(MainTexId, cameraColorTarget);
#endif
                _propertyBlock.SetTexture(OpaqueOnlyId, _opaqueOnlySource.Texture);
                _propertyBlock.SetVector(ReactiveParamsId, new Vector3(autoReactiveSettings.scale, autoReactiveSettings.cutoffThreshold, autoReactiveSettings.binaryValue));
                _propertyBlock.SetInt(ReactiveFlagsId, (int)autoReactiveSettings.flags);

                cmd.DrawProcedural(Matrix4x4.identity, autoReactiveMaterial, 0, MeshTopology.Triangles, 3, viewCount, _propertyBlock);
            }
            else
            {
                cmd.ClearRenderTarget(false, true, Color.clear);
            }

            // Reset the camera color render target for subsequent passes
            CoreUtils.SetRenderTarget(cmd, cameraColorTarget);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
#endif
    }
}
