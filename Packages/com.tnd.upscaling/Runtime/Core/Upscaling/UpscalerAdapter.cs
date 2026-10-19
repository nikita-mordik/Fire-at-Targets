#if TND_SRP_CORE && ENABLE_UPSCALER_FRAMEWORK
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace TND.Upscaling.Framework
{
    public class UpscalerAdapter<TUpscalerPlugin, TUpscaler, TSettings>: AbstractUpscaler
        where TUpscalerPlugin : UpscalerPlugin<TUpscaler, TSettings>, new()
        where TUpscaler: UpscalerBase<TSettings>
        where TSettings: UpscalerSettingsBase
    {
        public override string name => _upscalerPlugin.UpscalerRegistryName;
        public override bool isTemporal => _upscalerPlugin.IsTemporalUpscaler;
        public override bool supportsSharpening => true;    // All TND upscalers have built-in sharpening support
        public override bool supportsXR => false;   // TODO: TND upscalers should all support XR, but we'll ignore this for now
        public override UpscalerOptions options => _upscalerOptions;

        private readonly BaseRenderFunc<PassData, UnsafeGraphContext> _executePass;
        private readonly TUpscalerPlugin _upscalerPlugin;
        private readonly UpscalerAdapterOptions _upscalerOptions;
        private readonly TSettings _advancedSettings;
        private IUpscaler _upscaler;

        private Vector2Int _preUpscaleResolution;
        private Vector2Int _postUpscaleResolution;
        private Vector2 _jitterOffset;
        
        private Vector2Int _previousPostUpscaleResolution;
        private UpscalerQuality _previousQualityMode;

        public UpscalerAdapter(UpscalerOptions upscalerOptions)
        {
            _executePass = ExecutePass;
            
            _upscalerPlugin = new TUpscalerPlugin();
            _upscalerOptions = upscalerOptions as UpscalerAdapterOptions;
            _advancedSettings = _upscalerPlugin.CreateSettings() as TSettings;
        }

        class PassData
        {
            public bool createContext;
            
            public UpscalerInitParams initParams;
            public UpscalerDispatchParams dispatchParams;

            public TextureHandle inputColor;
            public TextureHandle inputDepth;
            public TextureHandle inputMotionVectors;
            public TextureHandle inputExposure;
            public TextureHandle outputColor;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UpscalingIO io = frameData.Get<UpscalingIO>();
            _preUpscaleResolution = io.preUpscaleResolution;
            _postUpscaleResolution = io.postUpscaleResolution;
            
            TextureHandle outputColor;
            {
                TextureDesc inputDesc = io.cameraColor.GetDescriptor(renderGraph);
                TextureDesc outputDesc = inputDesc;
                outputDesc.width = io.postUpscaleResolution.x;
                outputDesc.height = io.postUpscaleResolution.y;
                outputDesc.format = inputDesc.format;
                outputDesc.msaaSamples = MSAASamples.None;
                outputDesc.useMipMap = false;
                outputDesc.autoGenerateMips = false;
                outputDesc.useDynamicScale = false;
                outputDesc.anisoLevel = 0;
                outputDesc.discardBuffer = false;
                outputDesc.enableRandomWrite = _upscaler?.RequiresRandomWriteOutput ?? true;
                outputDesc.name = "_UpscalerOutput";
                outputDesc.clearBuffer = false;
                outputDesc.filterMode = FilterMode.Point;
                outputColor = renderGraph.CreateTexture(outputDesc);
            }

            using (var builder = renderGraph.AddUnsafePass("TND Upscaling", out PassData passData))
            {
                // TODO: for multi-view XR, we need to add more than one of these passes, each with their own view index
                
                passData.createContext = ShouldResetUpscaler(io);
                if (passData.createContext)
                {
                    Vector2Int maxRenderSize = io.postUpscaleResolution;
                    NegotiatePreUpscaleResolution(ref maxRenderSize, io.postUpscaleResolution);
                    
                    ref var init = ref passData.initParams;
                    init.camera = null; // TODO: need to obtain the camera from somewhere. We have io.cameraInstanceID to help us.
                    init.useTextureArrays = io.enableTexArray;
                    init.numTextureSlices = io.numActiveViews;
                    init.maxRenderSize = maxRenderSize;
                    init.upscaleSize = io.postUpscaleResolution;
                    init.enableHDR = io.hdrInput;
                    init.invertedDepth = io.invertedDepth;
                    init.highResMotionVectors = io.motionVectorTextureSize == io.postUpscaleResolution;
                    init.jitteredMotionVectors = io.jitteredMotionVectors;
                }
                
                float motionVectorSign = io.motionVectorDirection == UpscalingIO.MotionVectorDirection.PreviousFrameToCurrentFrame ? -1.0f : 1.0f;
                Vector2 motionVectorScale = io.motionVectorDomain == UpscalingIO.MotionVectorDomain.NDC ? new Vector2(io.motionVectorTextureSize.x, io.motionVectorTextureSize.y) : Vector2.one;
                
                ref var exec = ref passData.dispatchParams;
                exec.nonJitteredProjectionMatrix = io.projectionMatrices[0];    // TODO: this is not non-jittered
                exec.viewIndex = 0;
                
                exec.renderSize = io.preUpscaleResolution;
                exec.motionVectorScale = motionVectorSign * motionVectorScale;
                exec.jitterOffset = _jitterOffset;
                exec.preExposure = io.preExposureValue;
                exec.resetHistory = io.resetHistory;
                if (_upscalerOptions != null)
                {
                    exec.enableSharpening = _upscalerOptions.EnableSharpening;
                    exec.sharpness = _upscalerOptions.Sharpness;
                }
                else
                {
                    exec.enableSharpening = false;
                    exec.sharpness = 0.0f;
                }

                passData.inputColor = io.cameraColor;
                passData.inputDepth = io.cameraDepth;
                passData.inputMotionVectors = io.motionVectorColor;
                passData.inputExposure = io.exposureTexture;
                passData.outputColor = outputColor;
                
                builder.UseTexture(passData.inputColor);
                builder.UseTexture(passData.inputDepth);
                builder.UseTexture(passData.inputMotionVectors);
                if (passData.inputExposure.IsValid()) builder.UseTexture(passData.inputExposure);
                builder.UseTexture(passData.outputColor, AccessFlags.Write);
                
                builder.SetRenderFunc(_executePass);
            }

            io.cameraColor = outputColor;

            _previousPostUpscaleResolution = _postUpscaleResolution;
            if (_upscalerOptions != null)
            {
                _previousQualityMode = _upscalerOptions.QualityMode;
            }

            if (_advancedSettings != null)
            {
                _advancedSettings.UpdateCachedValues();
            }
        }

        private void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
            
            if (data.createContext)
            {
                if (_upscaler == null)
                {
                    if (!_upscalerPlugin.TryCreateUpscaler(cmd, _advancedSettings, data.initParams, out _upscaler))
                    {
                        Debug.LogWarning("Failed to create upscaler instance!");
                        return;
                    }
                }
                else
                {
                    _upscaler.Destroy(cmd);
                    if (!_upscaler.Initialize(cmd, data.initParams))
                    {
                        Debug.LogWarning("Failed to reinitialize upscaler instance!");
                        return;
                    }
                }
            }
            
            // Get the actual Texture objects from the texture handles and convert them to TextureRefs for the TND Upscaler framework
            ref var exec = ref data.dispatchParams;
            exec.inputColor = new TextureRef(data.inputColor);
            exec.inputDepth = new TextureRef(data.inputDepth);
            exec.inputMotionVectors = new TextureRef(data.inputMotionVectors);
            exec.inputExposure = new TextureRef(data.inputExposure);
            exec.inputReactiveMask = TextureRef.Null;   // TODO: IUpscaler FW doesn't provide any reactive mask, so we could maybe inject our own here
            exec.inputOpaqueOnly = TextureRef.Null;     // TODO: as above
            exec.outputColor = new TextureRef(data.outputColor);
            
            _upscaler.Dispatch(cmd, data.dispatchParams);
        }

        public override void CalculateJitter(int frameIndex, out Vector2 jitter, out bool allowScaling)
        {
            if (_upscaler != null)
            {
                jitter = _upscaler.GetJitterOffset(frameIndex, _preUpscaleResolution.x, _postUpscaleResolution.x);
                allowScaling = false;
            }
            else
            {
                base.CalculateJitter(frameIndex, out jitter, out allowScaling);
            }

            _jitterOffset = jitter;
        }

        public override void NegotiatePreUpscaleResolution(ref Vector2Int preUpscaleResolution, Vector2Int postUpscaleResolution)
        {
            float scale = _upscalerOptions?.QualityMode switch
            {
                UpscalerQuality.NativeAA => 1.0f,
                UpscalerQuality.UltraQuality => 1.2f,
                UpscalerQuality.Quality => 1.5f,
                UpscalerQuality.Balanced => 1.7f,
                UpscalerQuality.Performance => 2.0f,
                UpscalerQuality.UltraPerformance => 3.0f,
                _ => 1.0f,
            };

            preUpscaleResolution.x = Mathf.CeilToInt(postUpscaleResolution.x / scale);
            preUpscaleResolution.y = Mathf.CeilToInt(postUpscaleResolution.y / scale);
        }

        private bool ShouldResetUpscaler(UpscalingIO io)
        {
            if (_upscaler == null)
                return true;
            
            // TODO: there's a fundamental flaw in the IUpscaler framework's design here:
            // by using one upscaler instance across the entire render pipeline, if we have multiple cameras with different resolutions,
            // then we end up destroying and recreating the upscaler context on every invocation, as the resolution values change between cameras.
            if (io.postUpscaleResolution != _previousPostUpscaleResolution)
                return true;

            if (_upscalerOptions != null)
            {
                if (_upscalerOptions.QualityMode != _previousQualityMode)
                    return true;
            }

            if (_advancedSettings != null)
            {
                if (_advancedSettings.RestartRequired())
                    return true;
            }
            
            return false;
        }
    }

    // TODO: making this object the base object for UpscalerSettingsBase means we also add these common fields to the TNDUpscaler's advanced settings list, which is not what we want.
    // Unfortunately we cannot create generic scriptable objects, because Unity won't instantiate them. So a generic wrapper for any settings object also isn't possible.
    // For now we keep these common options and the per-upscaler advanced settings separate, but this means we cannot expose the advanced settings in Unity's UI at the moment.
    // For most upscalers this is okay, but e.g. SGSR2 is an issue because it means you can't select which variant you want to use.
    [Serializable]
    public class UpscalerAdapterOptions : UpscalerOptions
    {
        [SerializeField]
        private UpscalerQuality qualityMode = UpscalerQuality.Quality;

        [SerializeField]
        private bool enableSharpening = true;

        [SerializeField, Range(0.0f, 1.0f)]
        private float sharpness = 0.5f;

        public UpscalerQuality QualityMode
        {
            get => qualityMode;
            set => qualityMode = value;
        }

        public bool EnableSharpening
        {
            get => enableSharpening;
            set => enableSharpening = value;
        }

        public float Sharpness
        {
            get => sharpness;
            set => sharpness = value;
        }
    }
}
#endif
