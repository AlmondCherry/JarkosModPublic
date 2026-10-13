sampler uImage0 : register(s0);

float uTime;
float3 uColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 rel = coords - float2(0.5, 0.5);
    float dist = length(rel);
    
    
    float alpha = saturate((0.5 - dist) * 20.0);
    if (alpha <= 0.0) return float4(0, 0, 0, 0);
    
    
    float z = sqrt(max(0.0, 0.25 - dist * dist));
    float3 normal = normalize(float3(rel.x, rel.y, z));
    
    
    float2 sphereUV = float2(
        0.5 + atan2(normal.x, normal.z) / 6.2831853 + uTime * 0.1,
        0.5 - asin(normal.y) / 3.1415926 + uTime * 0.05
    );
    
    float noise = tex2D(uImage0, sphereUV).r;
    
    
    float fresnel = pow(1.0 - z * 2.0, 1.5);
    
    
    float3 col = uColor * noise * 0.7 + uColor * fresnel * 1.5;
    
    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
