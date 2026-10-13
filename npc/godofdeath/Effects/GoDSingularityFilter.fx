sampler uImage0 : register(s0);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float uOpacity;
float uProgress;
float uIntensity;
float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 res = max(uScreenResolution, float2(1.0, 1.0));
    
    
    float2 center = (uTargetPosition - uScreenPosition) / res;
    float2 p = coords - center;
    p.x *= res.x / res.y; 
    
    float dist = length(p);
    
    
    float radius = 0.08 * max(uProgress, 0.01);
    
    
    float warp = 0.0;
    if (dist > radius)
    {
        
        warp = 0.035 * uOpacity * (radius / dist) * (1.0 - smoothstep(radius, radius * 3.2, dist));
    }
    
    float2 distortedCoords = coords - normalize(p) * warp;
    distortedCoords = clamp(distortedCoords, float2(0.001, 0.001), float2(0.999, 0.999));
    
    
    if (dist < radius)
    {
        return float4(0.0, 0.0, 0.0, 1.0);
    }
    
    float4 baseColor = tex2D(uImage0, distortedCoords);
    
    
    float diskEnd = radius * 1.6;
    if (dist < diskEnd)
    {
        float ringFactor = 1.0 - ((dist - radius) / (diskEnd - radius));
        float angle = atan2(p.y, p.x);
        float noise = sin(angle * 5.0 - uTime * 8.0) * 0.25 + 0.75;
        
        
        baseColor.rgb += float3(0.5, 0.1, 0.95) * ringFactor * uOpacity * noise;
    }
    
    
    float edgeDist = length(coords - float2(0.5, 0.5));
    float vignette = smoothstep(0.85, 0.4, edgeDist);
    baseColor.rgb *= lerp(1.0, vignette * 0.6 + 0.4, uOpacity);
    
    return baseColor;
}

technique Technique1
{
    pass GoDSingularity
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
