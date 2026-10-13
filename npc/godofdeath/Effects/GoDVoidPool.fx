sampler uImage0 : register(s0);
float3 uColor;
float uOpacity;
float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    if (baseColor.a == 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    
    float bubble1 = sin(coords.x * 12.0 + uTime * 2.8) * cos(coords.y * 12.0 - uTime * 2.2);
    float bubble2 = cos(coords.x * 25.0 - uTime * 4.2) * sin(coords.y * 25.0 + uTime * 3.4);
    float combinedNoise = (bubble1 * 0.65 + bubble2 * 0.35) * 0.5 + 0.5;
    
    
    float3 liquidColor = float3(0.03, 0.0, 0.07);
    
    
    float3 bubbleColor = float3(0.52, 0.08, 0.88);
    
    
    float bubbleThreshold = smoothstep(0.70, 0.82, combinedNoise);
    float3 finalColor = lerp(liquidColor, bubbleColor, bubbleThreshold);
    
    
    float edgeGlow = smoothstep(0.01, 0.18, baseColor.a) * (1.0 - baseColor.a);
    finalColor += float3(0.7, 0.2, 0.95) * edgeGlow * uOpacity;
    
    return float4(finalColor, baseColor.a * uOpacity) * color;
}

technique Technique1
{
    pass GoDVoidPool
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
