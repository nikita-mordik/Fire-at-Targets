using UnityEngine;
using TND.Upscaling.Framework;

namespace TND.Upscaling.SGSR1
{
    public class SGSR1UpscalerSettings: UpscalerSettingsBase
    {
        public bool useEdgeDirection = true;

        [Range(4.0f, 8.0f)]
        public float edgeThreshold = 8.0f;

        [Range(1.0f, 2.0f)]
        public float edgeSharpness = 2.0f;
    }
}
