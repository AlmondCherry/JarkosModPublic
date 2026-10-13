sampler uImage0 : register(s0);
float3 uColor;
float uOpacity;
float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    
    float2 center = float2(0.5, 0.5);
    float2 p = coords - center;
    float dist = length(p);
    
    
    float mask = smoothstep(0.5, 0.42, dist);
    if (mask <= 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    
    float angle = atan2(p.y, p.x);
    float spiral = sin(angle * 3.0 + dist * 18.0 - uTime * 6.0) * 0.5 + 0.5;
    
    
    float coreMask = smoothstep(0.1, 0.28, dist);
    
    
    float3 portalColor = lerp(float3(0.03, 0.0, 0.08), float3(0.55, 0.05, 0.85), spiral * coreMask);
    
    
    float rim = smoothstep(0.38, 0.48, dist);
    portalColor += float3(0.75, 0.25, 1.0) * rim;
    
    return float4(portalColor, mask * uOpacity) * color;
}

technique Technique1
{
    pass GoDVoidRift
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
