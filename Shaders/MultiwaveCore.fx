sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 rel = coords - float2(0.5, 0.5);
    float r = length(rel);

    
    if (r > 0.5)
    {
        return float4(0.0, 0.0, 0.0, 0.0);
    }

    
    float theta = atan2(rel.y, rel.x);
    float thetaNorm = (theta + 3.14159265) / 6.2831853;

    
    
    float2 uv1 = float2(thetaNorm * 3.0 + uTime * 0.45, r * 1.8 - uTime * 0.9);
    float n1 = tex2D(uImage0, uv1).r;

    
    float2 uv2 = float2(thetaNorm * 4.0 - uTime * 0.35, r * 2.6 - uTime * 1.3);
    float n2 = tex2D(uImage0, uv2).g;

    
    float noise = saturate(n1 * 0.55 + n2 * 0.45);

    
    float edgeWobble = (noise - 0.5) * 0.08;
    float sunMask = saturate((0.48 + edgeWobble - r) * 12.0);

    
    float coreMask = saturate((0.16 - r) * 12.0);

    
    float glow = saturate((0.38 - r) * 3.0) * noise;

    
    float3 col = lerp(uSecondaryColor, uColor, sunMask * (0.3 + noise * 0.7));
    col += uColor * glow * 1.5;
    
    
    float3 hotCoreColor = float3(1.0, 1.0, 0.85);
    col = lerp(col, hotCoreColor, coreMask + coreMask * noise * 0.4);

    float alpha = sunMask * uOpacity;

    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
