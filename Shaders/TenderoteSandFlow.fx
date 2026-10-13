sampler uImage0 : register(s0);

float uTime;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 st = coords;
    
    
    float2 uv1 = st * 1.5 + float2(uTime * 2.0, uTime * 0.1);
    float2 uv2 = st * 3.0 + float2(uTime * 1.2, -uTime * 0.05);
    
    float noise1 = tex2D(uImage0, uv1).r;
    float noise2 = tex2D(uImage0, uv2).g;
    
    
    float sand = pow(noise1 * noise2, 1.5) * 3.0;
    sand = saturate(sand);
    
    
    float3 col = lerp(float3(0.02, 0.02, 0.02), float3(1.0, 1.0, 1.0), sand);
    
    return float4(col * uOpacity, uOpacity);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
