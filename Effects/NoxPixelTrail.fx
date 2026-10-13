sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uProgress;

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float along = coords.x;
    float across = coords.y;

    float edge = saturate(1.0 - abs(across - 0.5) * 2.0);
    float tail = saturate(along);

    float flame = sin(along * 18.0 - uTime * 4.0) * 0.5 + 0.5;
    float alpha = edge * tail * uOpacity;

    float3 color = lerp(uSecondaryColor, uColor, edge);
    color += flame * 0.15;

    return float4(color * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass NoxPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
