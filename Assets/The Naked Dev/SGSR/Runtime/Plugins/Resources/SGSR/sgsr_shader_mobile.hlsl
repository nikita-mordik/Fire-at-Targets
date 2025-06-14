//============================================================================================================
//
//
//                  Copyright (c) 2023, Qualcomm Innovation Center, Inc. All rights reserved.
//                              SPDX-License-Identifier: BSD-3-Clause
//
//============================================================================================================

#define SGSR_MOBILE

float4 ViewportInfo;
float EdgeSharpness;

struct VertexShaderOutput
{
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
};

VertexShaderOutput VS_main(
	float4 position : POSITION,
	float2 uv : TEXCOORD)
{
	VertexShaderOutput output;

	output.position = position;
	output.uv = uv;

	return output;
}

#define  SGSR_H 1

half4 SGSRRH(float3 p)
{
    half4 res = _BlitTexture.GatherRed(sampler_LinearClamp, p.xy);
    return res;
}
half4 SGSRGH(float3 p)
{
    half4 res = _BlitTexture.GatherGreen(sampler_LinearClamp, p.xy);
    return res;
}
half4 SGSRBH(float3 p)
{
    half4 res = _BlitTexture.GatherBlue(sampler_LinearClamp, p.xy);
    return res;
}
half4 SGSRAH(float3 p)
{
    half4 res = _BlitTexture.GatherAlpha(sampler_LinearClamp, p.xy);
    return res;
}
half4 SGSRRGBH(float3 p)
{ 
    half4 res = _BlitTexture.SampleLevel(sampler_LinearClamp, p.xy, 0);
    return res; 
}

half4 SGSRH(float2 p, uint channel)
{
    if (channel == 0)
        return SGSRRH(float3(p, 0));
    if (channel == 1)
        return SGSRGH(float3(p, 0));
    if (channel == 2)
        return SGSRBH(float3(p, 0));
    return SGSRAH(float3(p, 0));
}

#include "./sgsr_mobile.h"
// =====================================================================================
// 
// SNAPDRAGON GAME SUPER RESOLUTION
// 
// =====================================================================================
half4 SnapdragonGameSuperResolution(float2 uv)
{
	half4 OutColor = half4(0, 0, 0, 1);
    SgsrYuvH(OutColor, uv, ViewportInfo);
    return OutColor;
}

half4 PS_main (float4 position : SV_POSITION, float2 uv : TEXCOORD) : SV_TARGET
{
    return SnapdragonGameSuperResolution(uv);
}