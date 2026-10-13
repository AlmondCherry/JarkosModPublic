sampler uImage0 : register(s0);
float3 uColor;
float uOpacity;
float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    
    
    float distToCenter = abs(coords.y - 0.5);
    
    
    float widthMask = smoothstep(0.5, 0.01, distToCenter);
    
    
    float lengthMask = smoothstep(0.0, 0.12, coords.x) * smoothstep(1.0, 0.88, coords.x);
    
    float mask = widthMask * lengthMask;
    if (mask <= 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    
    float slitMask = smoothstep(0.08, 0.02, distToCenter);
    
    
    float glowMask = smoothstep(0.42, 0.08, distToCenter) * (1.0 - slitMask);
    
    
    float ripple = sin(coords.x * 20.0 - uTime * 12.0) * 0.35 + 0.65;
    
    float3 finalColor = float3(0.0, 0.0, 0.0);
    if (slitMask > 0.0)
    {
        finalColor = float3(0.02, 0.0, 0.05); 
    }
    
    finalColor += float3(0.65, 0.15, 0.95) * glowMask * ripple;
    
    return float4(finalColor, mask * uOpacity) * color;
}

technique Technique1
{
    pass GoDRiftSlash
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
