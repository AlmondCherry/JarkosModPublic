sampler uImage0 : register(s0);
float3 uColor;
float uOpacity;
float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    if (baseColor.a == 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    
    float wave = sin(coords.y * 8.0 - uTime * 5.0) * 0.5 + 0.5;
    float3 glowColor = lerp(float3(0.15, 0.02, 0.3), float3(0.6, 0.05, 0.95), wave);
    
    
    float edge = 1.0 - baseColor.a; 
    
    float4 finalColor = baseColor;
    finalColor.rgb = lerp(finalColor.rgb * color.rgb, glowColor, uOpacity);
    finalColor.rgb += float3(0.3, 0.1, 0.5) * edge * uOpacity;
    
    return finalColor * color.a;
}

technique Technique1
{
    pass GoDScytheGlow
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
