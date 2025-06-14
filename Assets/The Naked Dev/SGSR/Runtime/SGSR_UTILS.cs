using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TND.SGSR
{
    public static class SGSR_UTILS
    {
        public static readonly int idEdgeSharpness = Shader.PropertyToID("EdgeSharpness");
        public static readonly int idViewportInfo = Shader.PropertyToID("ViewportInfo"); 
        public static readonly int idBlitTexture = Shader.PropertyToID("_BlitTexture");
        public static readonly int idScaleBias = Shader.PropertyToID("_BlitScaleBias");
    }
}
