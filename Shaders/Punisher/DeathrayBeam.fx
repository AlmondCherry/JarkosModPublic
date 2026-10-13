sampler uImage0 : register(s0);
float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    
    float distFromCenter = coords.y - 0.5;
    if (distFromCenter < 0.0) distFromCenter = -distFromCenter;
    
    
    float uvMask = 1.0 - distFromCenter * 2.0;
    if (uvMask < 0.0) uvMask = 0.0;
    uvMask = uvMask * uvMask; 
    
    
    float waveVal = sin(coords.x * 60.0 - uTime * 25.0);
    float waveMod = 0.02 * waveVal; 
    
    
    float startTaper = 1.0;
    if (coords.x < 0.15)
    {
        float t = coords.x / 0.15;
        startTaper = t * (2.0 - t); 
    }
    
    
    float endTaper = 1.0;
    if (coords.x > 0.85)
    {
        float et = (1.0 - coords.x) / 0.15;
        if (et < 0.0) et = 0.0;
        if (et > 1.0) et = 1.0;
        endTaper = et * et * (3.0 - 2.0 * et);
    }
    
    float totalTaper = startTaper * endTaper;
    
    
    float edgeThreshold = 0.45 * totalTaper + waveMod * totalTaper;
    if (edgeThreshold < 0.01) edgeThreshold = 0.01;
    
    float normDist = distFromCenter / edgeThreshold;
    float nDist = normDist;
    if (nDist > 1.0) nDist = 1.0; 
    
    
    float whiteCore = exp(-nDist * nDist * 10.0);
    
    
    float3 purpleColor = float3(0.65, 0.0, 1.0); 
    
    
    float3 finalRGB;
    finalRGB.r = purpleColor.r * (1.0 - whiteCore) + 1.5 * whiteCore;
    finalRGB.g = purpleColor.g * (1.0 - whiteCore) + 1.5 * whiteCore;
    finalRGB.b = purpleColor.b * (1.0 - whiteCore) + 1.5 * whiteCore;
    
    
    
    float alphaFactor = 1.0 - nDist;
    float alpha = alphaFactor * alphaFactor * alphaFactor * totalTaper * uvMask;
    
    return float4(finalRGB * alpha, alpha);
}

technique Technique1
{
    pass BeamPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
