using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TND.SGSR
{
    public class SGSR_BIRP : SGSR_BASE
    {
        private CommandBuffer _SGSRPass;

        private RenderTexture _colorBuffer;
        private Material _blitMaterial;

        private int _displayWidth, _displayHeight;
        private int _renderWidth, _renderHeight;
        private float _previousEdgeSharpness;

        private float _prevScaleFactor;

        private Rect _originalRect;

        //Mipmap variables
        [Header("Mipmap Settings")]
        public bool autoTextureUpdate = true;
        [Range(0f, 1f)]
        public float mipmapBiasOverride = 0.5f;
        public float mipMapUpdateFrequency = 2f;

        protected Texture[] m_allTextures;
        protected ulong m_previousLength;
        protected float m_mipMapBias;
        protected float m_prevMipMapBias;
        protected float m_mipMapTimer = float.MaxValue;

        private void Awake()
        {
            _blitMaterial = new Material(Shader.Find("Hidden/SGSR_BlitShader_BIRP"));
        }

        protected override void InitializeSGSR()
        {
            base.InitializeSGSR();

            _SGSRPass = new CommandBuffer { name = "SGSR: Blit Pass" };

            SetupCommandBuffers();

            if (!_sgsrInitialized)
            {
                Camera.onPreRender += OnPreRenderCamera;
                Camera.onPostRender += OnPostRenderCamera;
            }

            _sgsrInitialized = true;
        }

        private void SetupCommandBuffers()
        {
            _prevScaleFactor = _scaleFactor;
            _previousEdgeSharpness = edgeSharpness;
            _displayWidth = Display.main.renderingWidth;
            _displayHeight = Display.main.renderingHeight;

            _originalRect = m_mainCamera.pixelRect;

            _renderWidth = (int)(_displayWidth / _scaleFactor);
            _renderHeight = (int)(_displayHeight / _scaleFactor);

            if (_colorBuffer)
            {
                _colorBuffer.Release();
            }

            _colorBuffer = new RenderTexture(_renderWidth, _renderHeight, 0, RenderTextureFormat.Default);
            _colorBuffer.Create();

            if (_SGSRPass != null)
            {
                m_mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _SGSRPass);
            }

            _SGSRPass.Clear();

            _SGSRPass.Blit(BuiltinRenderTextureType.CameraTarget, _colorBuffer);

            _SGSRPass.SetGlobalFloat(SGSR_UTILS.idEdgeSharpness, edgeSharpness);
            _SGSRPass.SetGlobalVector(SGSR_UTILS.idViewportInfo, new Vector4(1.0f / _renderWidth, 1.0f / _renderHeight, _renderWidth, _renderHeight));
            _SGSRPass.SetGlobalTexture(SGSR_UTILS.idBlitTexture, _colorBuffer);
            _SGSRPass.SetRenderTarget(BuiltinRenderTextureType.None, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            _SGSRPass.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            _SGSRPass.SetViewport(new Rect(Vector2.zero, new Vector2(_displayWidth, _displayHeight)));
            _SGSRPass.DrawMesh(FullscreenMesh, Matrix4x4.identity, _blitMaterial, 0, 0);
            _SGSRPass.SetViewProjectionMatrices(m_mainCamera.worldToCameraMatrix, m_mainCamera.projectionMatrix);

            m_mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _SGSRPass);
        }

        private void OnPreRenderCamera(Camera camera)
        {
            if (camera == m_mainCamera)
            {
                if (_displayWidth != Display.main.renderingWidth ||
                    _displayHeight != Display.main.renderingHeight ||
                    _previousEdgeSharpness != edgeSharpness ||
                    _prevScaleFactor != _scaleFactor)
                {
                    SetupCommandBuffers();
                }

                m_mainCamera.pixelRect = new Rect(0, 0, _colorBuffer.width, _colorBuffer.height);
            }

        }

        private void OnPostRenderCamera(Camera camera)
        {
            if (camera == m_mainCamera)
            {
                m_mainCamera.pixelRect = _originalRect;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (autoTextureUpdate)
            {
                UpdateMipMaps();
            }
        }
        protected override void DisableSGSR()
        {
            base.DisableSGSR();

            Camera.onPreRender -= OnPreRenderCamera;
            Camera.onPostRender -= OnPostRenderCamera;

            OnResetAllMipMaps();

            if (m_mainCamera != null)
            {
                m_mainCamera.pixelRect = _originalRect;

                if (_SGSRPass != null)
                {
                    m_mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _SGSRPass);
                }
            }

            _SGSRPass = null;

            if (_colorBuffer)
            {
                _colorBuffer.Release();
            }
        }

        /// <summary>
        /// Updates a single texture to the set MipMap Bias.
        /// Should be called when an object is instantiated, or when the ScaleFactor is changed.
        /// </summary>
        public void OnMipmapSingleTexture(Texture texture)
        {
            texture.mipMapBias = m_mipMapBias;
        }

        /// <summary>
        /// Updates all textures currently loaded to the set MipMap Bias.
        /// Should be called when a lot of new textures are loaded, or when the ScaleFactor is changed.
        /// </summary>
        public void OnMipMapAllTextures()
        {
            m_allTextures = Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[];
            for (int i = 0; i < m_allTextures.Length; i++)
            {
                m_allTextures[i].mipMapBias = m_mipMapBias;
            }
        }
        /// <summary>
        /// Resets all currently loaded textures to the default mipmap bias.
        /// </summary>
        public void OnResetAllMipMaps()
        {
            m_prevMipMapBias = -1;

            m_allTextures = Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[];
            for (int i = 0; i < m_allTextures.Length; i++)
            {
                m_allTextures[i].mipMapBias = 0;
            }
            m_allTextures = null;
        }

        #region Automatic Mip Map
#if UNITY_BIRP
        /// <summary>
        /// Automatically updates the mipmap of all loaded textures
        /// </summary>
        protected void UpdateMipMaps()
        {
            m_mipMapTimer += Time.deltaTime;

            if (m_mipMapTimer > mipMapUpdateFrequency)
            {
                m_mipMapTimer = 0;

                m_mipMapBias = (Mathf.Log((float)(_renderWidth) / (float)(_displayWidth), 2f) - 1) * mipmapBiasOverride;

                if (m_previousLength != Texture.currentTextureMemory || m_prevMipMapBias != m_mipMapBias)
                {
                    m_prevMipMapBias = m_mipMapBias;
                    m_previousLength = Texture.currentTextureMemory;

                    OnMipMapAllTextures();
                }
            }
        }
#endif
        #endregion

        static Mesh _FullscreenMesh = null;
        /// <summary>
        /// Returns a mesh that you can use with <see cref="CommandBuffer.DrawMesh(Mesh, Matrix4x4, Material)"/> to render full-screen effects.
        /// </summary>
        public static Mesh FullscreenMesh
        {
            get
            {
                if (_FullscreenMesh != null)
                    return _FullscreenMesh;

                float topV = 1.0f;
                float bottomV = 0.0f;

                _FullscreenMesh = new Mesh { name = "Fullscreen Quad" };
                _FullscreenMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f)
                });

                _FullscreenMesh.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, bottomV),
                    new Vector2(0.0f, topV),
                    new Vector2(1.0f, bottomV),
                    new Vector2(1.0f, topV)
                });

                _FullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
                _FullscreenMesh.UploadMeshData(true);
                return _FullscreenMesh;
            }
        }
    }
}
