Shader "Hidden/TND/Upscaling/SGSR1_BlitShader"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass    // 0 - Fast
        {
            Name "SGSR1"
            
            HLSLPROGRAM
            #pragma vertex VertMain
            #pragma fragment frag
            #pragma target 4.5
            //#pragma enable_d3d11_debug_symbols

            #pragma multi_compile __ TND_USE_TEXARRAYS

            #include "SGSR1_BlitShader.hlsl"

            ENDHLSL
        }

        Pass    // 1 - With Edge Direction
        {
            Name "SGSR1 - Edge Direction"
            
            HLSLPROGRAM
            #pragma vertex VertMain
            #pragma fragment frag
            #pragma target 4.5
            //#pragma enable_d3d11_debug_symbols

            #pragma multi_compile __ TND_USE_TEXARRAYS

            #define UseEdgeDirection

            #include "SGSR1_BlitShader.hlsl"

            ENDHLSL
        }
    }

    Fallback "Hidden/TND/Upscaling/SGSR1_BlitShaderLow"
}
