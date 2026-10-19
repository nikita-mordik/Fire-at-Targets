using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TND.Upscaling.Framework
{
    /// <summary>
    /// This class provides the bridge between a Unity camera and an upscaler instance.
    /// Exactly one upscaler context object should exist per upscaling camera view, and this context manages the currently active upscaler for each view.
    /// Where this context should live and how it is associated with a camera depends on the specific details of the render pipeline integration.
    /// </summary>
    public class UpscalerContext
    {
        public IUpscaler ActiveUpscaler => _activeUpscaler;
        public IUpscalerPlugin ActiveUpscalerPlugin => _activeUpscalerPlugin;
        protected internal UpscalerController UpscalerController => _upscalerController;
        
        private UpscalerInitParams _initParams;
        
        private IUpscaler _activeUpscaler;
        private IUpscalerPlugin _activeUpscalerPlugin;
        private UpscalerController _upscalerController;
        private string _selectedUpscalerIdentifier;

        private ShaderRef _fallbackUpscaleShader;
        private Material _fallbackUpscaleMaterial;

        private ShaderRef _autoReactiveShader;
        private Material _autoReactiveMaterial;

        private ShaderRef _mergeReactiveShader;
        private Material _mergeReactiveMaterial;
        
        private GlobalKeyword _texArraysKeyword;
        private GlobalKeyword _srgbKeyword;

        internal static readonly string TexArraysKeyword = "TND_USE_TEXARRAYS";
        internal static readonly string SrgbKeyword = "TND_SRGB";
        internal Material AutoReactiveMaterial => UpscalerUtils.TryLoadMaterial("TND_AutoReactive", ref _autoReactiveMaterial, ref _autoReactiveShader) ? _autoReactiveMaterial : null;
        internal Material MergeReactiveMaterial => UpscalerUtils.TryLoadMaterial("TND_MergeReactive", ref _mergeReactiveMaterial, ref _mergeReactiveShader) ? _mergeReactiveMaterial : null;

        public virtual bool Initialize(CommandBuffer cmd, in UpscalerInitParams initParams)
        {
            _initParams = initParams;
            _texArraysKeyword = GlobalKeyword.Create(TexArraysKeyword);
            _srgbKeyword = GlobalKeyword.Create(SrgbKeyword);
            
            if (_initParams.camera == null)
                return false;
            
            _upscalerController = _initParams.camera.GetComponent<UpscalerController>();
            if (_upscalerController == null || !_upscalerController.isActiveAndEnabled)
                return false;

            // Let the upscaler controller know which upscaler context is attached to it.
            // This allows for some render pipeline-specific interactions.
            _upscalerController.UpscalerContext = this;
            
            IEnumerator<IUpscalerPlugin> fallbackChain = _upscalerController.EnumerateUpscalerChain();

            // Pick the appropriate upscaler plugin based on the selection in the UpscalerController script
            _selectedUpscalerIdentifier = _upscalerController.GetPrimaryUpscalerIdentifier();
            IUpscalerPlugin upscalerPlugin = UpscalerPluginRegistry.FindUpscalerPlugin(_selectedUpscalerIdentifier);

            while (true)
            {
                if (upscalerPlugin != null && upscalerPlugin.IsSupported)
                {
                    _upscalerController.TryGetUpscalerSettings(upscalerPlugin, out var upscalerSettings);
                    if (upscalerPlugin.TryCreateUpscaler(cmd, upscalerSettings ?? upscalerPlugin.CreateSettings(), initParams, out _activeUpscaler))
                    {
                        _activeUpscalerPlugin = upscalerPlugin;
                        UpscalerDebug.Log($"Initialized upscaler plugin of type: {_activeUpscalerPlugin.DisplayName}");
                        _upscalerController.EnableOpaqueOnlyCopy = GetOpaqueOnlyRequired();
                        return true;
                    }
                }

                // Go through the fallback chain in order until we find an upscaler that works
                if (!fallbackChain.MoveNext())
                {
                    _activeUpscaler = null;
                    _activeUpscalerPlugin = null;
                    return false;
                }

                upscalerPlugin = fallbackChain.Current;
            }
        }
        
        public virtual void Destroy(CommandBuffer cmd)
        {
            if (_activeUpscaler != null)
            {
                _activeUpscaler.Destroy(cmd);
                _activeUpscaler = null;
            }

            UpscalerUtils.UnloadMaterial(ref _autoReactiveMaterial, ref _autoReactiveShader);
            UpscalerUtils.UnloadMaterial(ref _mergeReactiveMaterial, ref _mergeReactiveShader);
            UpscalerUtils.UnloadMaterial(ref _fallbackUpscaleMaterial, ref _fallbackUpscaleShader);
        }
        
        public virtual void Execute(CommandBuffer cmd, ref UpscalerDispatchParams dispatchParams)
        {
            if (_upscalerController == null)
            {
                // Upscaler context may have been created without a controller script present, so check if one exists now
                _upscalerController = _initParams.camera.GetComponent<UpscalerController>();
            }
            
            if (_initParams.useTextureArrays)
                cmd.EnableKeyword(_texArraysKeyword);
            else
                cmd.DisableKeyword(_texArraysKeyword);
            
            if (UpscalerUtils.IsSRGBFormat(dispatchParams.inputColor.GraphicsFormat))
                cmd.EnableKeyword(_srgbKeyword);
            else
                cmd.DisableKeyword(_srgbKeyword);

            if (RestartRequired())
            {
                // Don't run the active upscaler on the frame where we switch, to allow cleanup to happen safely.
                // This prevents a whole host of potential errors and crashes from happening.
                ExecuteFallbackUpscaler(cmd, dispatchParams);
                Restart(cmd);
                return;
            }
            
            if (_activeUpscaler == null || _activeUpscalerPlugin == null)
            {
                ExecuteFallbackUpscaler(cmd, dispatchParams);
                return;
            }

            Vector2Int minRenderSize = _activeUpscaler.MinimumRenderSize;
            if (dispatchParams.renderSize.x < minRenderSize.x || dispatchParams.renderSize.y < minRenderSize.y)
            {
                ExecuteFallbackUpscaler(cmd, dispatchParams);
                return;
            }
            
            if (!dispatchParams.inputOpaqueOnly.IsValid)
            {
                // Default to the input color buffer, so any passes comparing input color against opaque-only will just end up doing nothing
                dispatchParams.inputOpaqueOnly = dispatchParams.inputColor;
            }

            if (_upscalerController != null)
            {
                dispatchParams.enableSharpening = _upscalerController.enableSharpening;
                dispatchParams.sharpness = _upscalerController.sharpness;
                
                // Enabling the opaque-only copy this late might mean we get the texture one frame late, but that shouldn't be a huge issue
                _upscalerController.EnableOpaqueOnlyCopy = GetOpaqueOnlyRequired();
            }
            
            // Unbind the last render target, as on DX11 any resource that is still bound as a render target cannot be used as a shader resource
            cmd.SetRenderTarget(BuiltinRenderTextureType.None);
            
            _activeUpscaler.Dispatch(cmd, dispatchParams);
        }

        private bool RestartRequired()
        {
            // Check if the selected upscaler has changed
            if (_upscalerController != null && _upscalerController.GetPrimaryUpscalerIdentifier() != _selectedUpscalerIdentifier)
            {
                return true;
            }

            // Check if settings for the currently active upscaler have changed significantly
            if (_activeUpscaler != null && _activeUpscaler.RestartRequired())
            {
                return true;
            }

            return false;
        }
        
        private void Restart(CommandBuffer cmd)
        {
            Destroy(cmd);
            Initialize(cmd, _initParams);
        }

        private bool GetOpaqueOnlyRequired()
        {
            return (_activeUpscalerPlugin.AcceptsReactiveMask && _upscalerController.autoGenerateReactiveMask) || _activeUpscaler.RequiresOpaqueOnlyInput;
        }

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int RenderSizeId = Shader.PropertyToID("_RenderSize");
        private static readonly int InvUpscaleSizeId = Shader.PropertyToID("_InvUpscaleSize");
        private static readonly int InvInputSizeId = Shader.PropertyToID("_InvInputSize");
        private static readonly int JitterOffsetId = Shader.PropertyToID("_JitterOffset");

        private void ExecuteFallbackUpscaler(CommandBuffer cmd, in UpscalerDispatchParams dispatchParams)
        {
            int depthSlice = dispatchParams.viewIndex;
            
            if (!UpscalerUtils.TryLoadMaterial("TND_FallbackUpscale", ref _fallbackUpscaleMaterial, ref _fallbackUpscaleShader))
            {
                cmd.Blit(dispatchParams.inputColor.GetRenderTargetIdentifier(depthSlice), dispatchParams.outputColor.GetRenderTargetIdentifier(depthSlice));
                return;
            }

            cmd.BeginSample("Fallback Upscale");
            cmd.SetGlobalTexture(MainTexId, dispatchParams.inputColor.GetRenderTargetIdentifier(depthSlice));
            cmd.SetGlobalVector(RenderSizeId, new Vector2(dispatchParams.renderSize.x, dispatchParams.renderSize.y));
            cmd.SetGlobalVector(InvUpscaleSizeId, new Vector2(1.0f / _initParams.upscaleSize.x, 1.0f / _initParams.upscaleSize.y));
            cmd.SetGlobalVector(InvInputSizeId, new Vector2(1.0f / dispatchParams.inputColor.Width, 1.0f / dispatchParams.inputColor.Height));
            cmd.SetGlobalVector(JitterOffsetId, dispatchParams.jitterOffset / dispatchParams.renderSize);
            cmd.SetRenderTarget(dispatchParams.outputColor.GetRenderTargetIdentifier(depthSlice), 0, CubemapFace.Unknown, depthSlice);
            cmd.DrawProcedural(Matrix4x4.identity, _fallbackUpscaleMaterial, 0, MeshTopology.Triangles, 3, 1);
            cmd.EndSample("Fallback Upscale");
        }
    }
}
