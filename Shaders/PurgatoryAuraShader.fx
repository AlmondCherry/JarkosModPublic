sampler uImage0 : register(s0);

float uTime;
float uOpacity;
float uSaturation;
float3 uColor;          
float3 uSecondaryColor; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 rel = coords - float2(0.5, 0.5);
    float dist = length(rel);
    
    
    float alpha = saturate((0.5 - dist) * 24.0);
    if (alpha <= 0.0) return float4(0, 0, 0, 0);
    
    
    float angle = atan2(rel.y, rel.x);
    float time = uSaturation > 0.001 ? uSaturation : uTime;
    
    
    float ringWave = sin(dist * 40.0 - time * 6.0) * 0.08 + cos(dist * 25.0 + time * 4.0) * 0.05;
    float edgeWave = sin(angle * 14.0 + time * 3.5) * 0.015 + cos(angle * 7.0 - time * 2.0) * 0.01;
    
    float currentDist = dist + edgeWave;

    
    float rim = pow(saturate(currentDist * 2.0), 2.4);

    
    float3 baseCol = uColor.rgb > 0.001 ? uColor : float3(1.0, 0.82, 0.2);
    float3 edgeCol = uSecondaryColor.rgb > 0.001 ? uSecondaryColor : float3(1.0, 0.96, 0.5);

    
    float energyPattern = 0.5 + ringWave + sin(angle * 8.0 + dist * 15.0 - time * 3.0) * 0.15;

    float3 finalColor = lerp(baseCol * energyPattern, edgeCol * 2.0, rim);
    float finalAlpha = alpha * (0.35 + rim * 0.65) * saturate(uOpacity > 0.001 ? uOpacity : 1.0);

    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
