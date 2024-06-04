﻿struct VertexInputType
{
    float4 position : POSITION;
    float3 normal : NORMAL0;
    float2 tex : TEXCOORD0;

    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;

    float4 Weights : COLOR;
    float4 BoneIndices : BLENDINDICES0;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;

    float3 normal : NORMAL0;
    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;

    float3 viewDirection : TEXCOORD1;
    float3 worldPosition : TEXCOORD5;

};
