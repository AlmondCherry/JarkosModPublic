sampler uImage0 : register(s0);

float uTime;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 delta = coords - float2(0.5, 0.5);
    float dist = length(delta);
    
    float alphaFade = 1.0 - smoothstep(0.3, 0.5, dist);
    if (dist > 0.5 || alphaFade <= 0.0) 
        return float4(0, 0, 0, 0);
        
    float angle = atan2(delta.y, delta.x);
    
    
    float bend = sin(dist * 15.0 - uTime * 4.0) * 0.6; 
    
    
    float bentAngle = angle + bend + (uTime * 1.5);
    
    
    float rays = sin(bentAngle * 12.0);
    rays = smoothstep(0.5, 0.9, rays);
    
    
    float core = 1.0 - smoothstep(0.0, 0.2, dist);
    
    float intensity = (rays + core * 1.5) * alphaFade;
    
    float3 beamColor = float3(0.5, 0.0, 0.7); 
    float3 coreColor = float3(1.0, 0.4, 1.0); 
    
    float3 finalColor = lerp(beamColor, coreColor, core);
    
    finalColor *= intensity * uOpacity;
    finalColor = saturate(finalColor);
    
    return float4(finalColor, (intensity * 0.8) * uOpacity) * color;
}

technique Technique1
{
    pass AuraRays
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
