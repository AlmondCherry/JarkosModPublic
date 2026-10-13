







sampler uImage0 : register(s0);

float uProgress;         
float uOpacity;          
float uTime;
float3 uColor;           
float3 uSecondaryColor;  

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 delta = coords - 0.5;
    float dist = length(delta) * 2.0;      

    if (dist > 1.0)
        return float4(0, 0, 0, 0);

    float front = uProgress;

    
    float frontBand = saturate(1.0 - abs(dist - front) / 0.035);
    frontBand = frontBand * frontBand;

    
    float behind = step(dist, front);
    float trail = saturate((front - dist) / 0.35) * behind;
    trail = trail * (1.0 - trail * 0.6);

    
    float ripple = sin(dist * 60.0 - uTime * 30.0) * 0.5 + 0.5;
    ripple *= trail * 0.15;

    
    
    float sparkPattern = sin((coords.x + coords.y) * 35.0 - uTime * 20.0) * sin((coords.x - coords.y) * 25.0 + uTime * 15.0);
    float sparks = saturate((sparkPattern - 0.4) * 3.0);
    sparks *= saturate(1.0 - abs(dist - front) / 0.08); 

    
    float3 col = lerp(uSecondaryColor, uColor, saturate(dist / max(front, 0.001)));
    col = col * (trail * 0.55 + ripple);
    col += float3(1, 1, 1) * frontBand * 1.25;
    col += lerp(uColor, float3(1, 1, 1), 0.5) * sparks * 0.9;

    
    float dissipate = 1.0 - uProgress * uProgress;
    float alpha = saturate(frontBand + trail * 0.55 + sparks) * dissipate * uOpacity;

    return float4(col * alpha, 0.0);
}

technique Technique1
{
    pass ShockwavePass
    {
        PixelShader = compile ps_2_b PixelShaderFunction();
    }
}
