#include "Packages/com.tnd.upscaling/Resources/TND_Common.hlsl"
            
TEXTURE2D _BlitTexture;
float4 _BlitTexture_TexelSize;
SamplerState sampler_LinearClamp;

#define SGSR_MOBILE

uniform float4 ViewportInfo;
uniform float EdgeSharpness;
uniform float EdgeThreshold = 8.0/255.0;

#define SGSR_H 1

half4 SGSRRH(float2 p)
{
#if SHADER_TARGET < 45
    float2 size = _BlitTexture_TexelSize.zw - 1;
    half4 res = half4(
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 1)) * size))].r,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 1)) * size))].r,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 0)) * size))].r,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 0)) * size))].r);
#else
    half4 res = _BlitTexture.GatherRed(sampler_LinearClamp, UV(p));
#endif
    return res;
}

half4 SGSRGH(float2 p)
{
#if SHADER_TARGET < 45
    float2 size = _BlitTexture_TexelSize.zw - 1;
    half4 res = half4(
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 1)) * size))].g,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 1)) * size))].g,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 0)) * size))].g,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 0)) * size))].g);
#else
    half4 res = _BlitTexture.GatherGreen(sampler_LinearClamp, UV(p));
#endif
    return res;
}

half4 SGSRBH(float2 p)
{
#if SHADER_TARGET < 45
    float2 size = _BlitTexture_TexelSize.zw - 1;
    half4 res = half4(
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 1)) * size))].b,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 1)) * size))].b,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 0)) * size))].b,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 0)) * size))].b);
#else
    half4 res = _BlitTexture.GatherBlue(sampler_LinearClamp, UV(p));
#endif
    return res;
}

half4 SGSRAH(float2 p)
{
#if SHADER_TARGET < 45
    float2 size = _BlitTexture_TexelSize.zw - 1;
    half4 res = half4(
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 1)) * size))].a,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 1)) * size))].a,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(1, 0)) * size))].a,
        _BlitTexture[COORD(int2(saturate(p + _BlitTexture_TexelSize.xy * float2(0, 0)) * size))].a);
#else
    half4 res = _BlitTexture.GatherAlpha(sampler_LinearClamp, UV(p));
#endif
    return res;
}

half4 SGSRRGBH(float2 p)
{ 
    half4 res = _BlitTexture.SampleLevel(sampler_LinearClamp, UV(p), 0);
    return res; 
}

half4 SGSRH(float2 p, uint channel)
{
    if (channel == 0)
        return SGSRRH(p);
    if (channel == 1)
        return SGSRGH(p);
    if (channel == 2)
        return SGSRBH(p);
    return SGSRAH(p);
}

#include "sgsr1_mobile.h"
            
half4 frag(VertexOut i) : SV_TARGET0
{
    half4 OutColor = half4(0, 0, 0, 1);
    SgsrYuvH(OutColor, i.texCoord, ViewportInfo);
    return OutColor;
}
