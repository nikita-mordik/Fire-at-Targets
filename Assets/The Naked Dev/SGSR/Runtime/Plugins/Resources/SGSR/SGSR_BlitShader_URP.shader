Shader "Hidden/SGSR_BlitShader_URP"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100
        ZWrite Off ZTest Always Blend Off Cull Off

        //Compatible with Unity 2022+
        Pass
        {
            HLSLPROGRAM 
             
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment SGSR_Frag
            //#pragma enable_d3d11_debug_symbols

            TEXTURE2D_X(_BlitTexture);
            uniform float4 _BlitScaleBias;

            #ifndef UNITY_CORE_SAMPLERS_INCLUDED
                SAMPLER(sampler_LinearClamp);
            #endif

            #if SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #else
            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #endif
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            
            #if SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
            #endif
            
                output.positionCS = pos;
                output.texcoord   = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
                return output;
            }

            #include "./sgsr_shader_mobile.hlsl"

            half4 SGSR_Frag (Varyings input) : SV_Target
            {
                return SnapdragonGameSuperResolution(input.texcoord);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
