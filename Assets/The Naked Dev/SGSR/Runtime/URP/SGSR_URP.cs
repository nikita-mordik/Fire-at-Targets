using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace TND.SGSR
{
    public class SGSR_URP : SGSR_BASE
    {
#if UNITY_2022_1_OR_NEWER
        internal RTHandle colorBuffer;
#else
        internal RenderTargetIdentifier colorBuffer;
#endif
        internal int actualdisplayWidth, actualdisplayHeight, displayWidth, displayHeight;
        internal Vector4 viewportInfo;

        private UniversalRenderPipelineAsset _universalRenderPipelineAsset;

        private float _prevScaleFactor = 0;
        private List<SGSRRenderFeature> _renderFeatures;
        private bool _containsRenderFeature = false;

        protected override void DisableSGSR()
        {
            base.DisableSGSR();

            RenderPipelineManager.beginCameraRendering -= PreRenderCamera;

            SetDynamicResolution(1);
            if (_renderFeatures != null)
            {
                foreach (SGSRRenderFeature renderFeature in _renderFeatures)
                {
                    renderFeature.isEnabled = false;
                }
            }
        }

        protected override void InitializeSGSR()
        {
            base.InitializeSGSR();

            SetupResolution();

            if (!_sgsrInitialized)
            {
                RenderPipelineManager.beginCameraRendering += PreRenderCamera;
            }

            _sgsrInitialized = true;
        }

        private void SetupResolution()
        {
            displayWidth = m_mainCamera.pixelWidth;
            displayHeight = m_mainCamera.pixelHeight;
      
            _prevScaleFactor = _scaleFactor;
            _containsRenderFeature = GetRenderFeature();
            SetDynamicResolution(_scaleFactor);

            if (!_containsRenderFeature)
            {
                Debug.LogError("Current Universal Render Data is missing the 'SGSR Render Pass URP' Rendering Feature");
            }
            else
            {
                foreach (SGSRRenderFeature renderFeature in _renderFeatures)
                {
                    renderFeature.OnSetReference(this);
                    renderFeature.isEnabled = true;
                }
            }
        }

        private void PreRenderCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != m_mainCamera)
            {
                return;
            }
            actualdisplayWidth = m_mainCamera.pixelWidth;
            actualdisplayHeight = m_mainCamera.pixelHeight;
       
            if (m_mainCamera.stereoEnabled)
            {
                // actualdisplayWidth = (int)(XRSettings.eyeTextureWidth / XRSettings.eyeTextureResolutionScale);
                // actualdisplayHeight = (int)(XRSettings.eyeTextureHeight / XRSettings.eyeTextureResolutionScale);
            }

            if (displayWidth != actualdisplayWidth || displayHeight != actualdisplayHeight || _prevScaleFactor != _scaleFactor)
            {
                SetupResolution();
            }
        }

        private bool GetRenderFeature()
        {
            _renderFeatures = new List<SGSRRenderFeature>();

            _universalRenderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            bool scriptableRenderFeatureFound = false;
            if (_universalRenderPipelineAsset != null)
            {
#if UNITY_2021_3_OR_NEWER
                _universalRenderPipelineAsset.upscalingFilter = UpscalingFilterSelection.Linear;
#endif

                Type type = _universalRenderPipelineAsset.GetType();
                FieldInfo fieldInfo = type.GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);

                if (fieldInfo != null)
                {
                    var scriptableRenderData = (ScriptableRendererData[])fieldInfo.GetValue(_universalRenderPipelineAsset);

                    if (scriptableRenderData != null && scriptableRenderData.Length > 0)
                    {
                        foreach (var renderData in scriptableRenderData)
                        {
                            foreach (var renderFeature in renderData.rendererFeatures)
                            {
                                SGSRRenderFeature sgsrFeature = renderFeature as SGSRRenderFeature;
                                if (sgsrFeature != null)
                                {
                                    _renderFeatures.Add(sgsrFeature);
                                    scriptableRenderFeatureFound = true;

                                    //Stop looping the current renderer, we only allow 1 instance per renderer 
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("SGSR: Can't find UniversalRenderPipelineAsset");
            }

            return scriptableRenderFeatureFound;
        }

        public void SetDynamicResolution(float value)
        {
            if (_universalRenderPipelineAsset != null)
            {
                float renderScale = 1.0f / value;
                _universalRenderPipelineAsset.renderScale = renderScale;

                float renderWidth = displayWidth * renderScale;
                float renderHeight = displayHeight * renderScale;
                viewportInfo = new Vector4(1.0f / renderWidth, 1.0f / renderHeight, renderWidth, renderHeight);
            }
        }
    }
}
