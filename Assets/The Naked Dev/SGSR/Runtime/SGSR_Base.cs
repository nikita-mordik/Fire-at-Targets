using UnityEngine;

namespace TND.SGSR
{
    public enum SGSR_Quality
    {
        Off = 0,
        NativeAA = 1,
        UltraQuality = 2,
        Quality = 3,
        Balanced = 4,
        Performance = 5,
        UltraPerformance = 6,
    }

    [RequireComponent(typeof(Camera))]
    public abstract class SGSR_BASE : MonoBehaviour
    {
        [Header("SGSR Settings")]
        public SGSR_Quality qualityLevel = SGSR_Quality.Performance;
        [Range(0.0f, 5.0f)]
        public float edgeSharpness = 2.0f;

        protected bool _sgsrInitialized = false;
        [HideInInspector]
        public Camera m_mainCamera;
        protected float _scaleFactor = 2f;
        protected SGSR_Quality _previousQuality;

        public void OnSetQuality(SGSR_Quality quality)
        {
            qualityLevel = _previousQuality = quality;

            if (quality == SGSR_Quality.Off)
            {
                if (_sgsrInitialized)
                {
                    DisableSGSR();
                }
            }
            else
            {
                switch (quality)
                {
                    case SGSR_Quality.NativeAA:
                        _scaleFactor = 1.0f;
                        break;
                    case SGSR_Quality.UltraQuality:
                        _scaleFactor = 1.2f;
                        break;
                    case SGSR_Quality.Quality:
                        _scaleFactor = 1.5f;
                        break;
                    case SGSR_Quality.Balanced:
                        _scaleFactor = 1.7f;
                        break;
                    case SGSR_Quality.Performance:
                        _scaleFactor = 2.0f;
                        break;
                    case SGSR_Quality.UltraPerformance:
                        _scaleFactor = 3.0f;
                        break;
                    default:
                        Debug.LogError($"[SGSR Upscaler]: Quality Level {quality} is not implemented");
                        break;
                }

                Initialize();
            }
        }

        protected void OnEnable()
        {
            OnSetQuality(qualityLevel);
        }

        protected virtual void Initialize()
        {
            if (_sgsrInitialized || !Application.isPlaying)
            {
                return;
            }

            InitializeSGSR();
        }

        protected virtual void InitializeSGSR()
        {
            m_mainCamera = GetComponent<Camera>();
        }

        protected void OnDisable()
        {
            DisableSGSR();
        }

        protected void OnDestroy()
        {
            DisableSGSR();
        }

        protected virtual void DisableSGSR()
        {
            _sgsrInitialized = false;
        }

        protected virtual void Update()
        {
            //Detection for if qualityLevel is changed outside of OnSetQualityLevel

            if (_previousQuality != qualityLevel)
            {
                OnSetQuality(qualityLevel);
            }
        }
    }
}
