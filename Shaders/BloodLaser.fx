sampler uImage0 : register(s0);
 
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float4 uShaderSpecificData;
 
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float x = coords.x;
    float y = coords.y * 2.0 - 1.0;
    float ay = abs(y);
 
    float speed = uShaderSpecificData.x;
    float capLength = uShaderSpecificData.y;
 
    float core = 1.0 - ay / 0.11;
    core = clamp(core, 0.0, 1.0);
    core = core * core;
 
    float body = 1.0 - ay / 0.42;
    body = clamp(body, 0.0, 1.0);
    body = body * body;
 
    float aura = 1.0 - ay / 1.0;
    aura = clamp(aura, 0.0, 1.0);
    aura = aura * aura;
 
    float band1 = frac(x * 18.0 - uTime * speed + y * 0.35);
    band1 = 1.0 - abs(band1 * 2.0 - 1.0);
    band1 = clamp((band1 - 0.45) / 0.55, 0.0, 1.0);
 
    float band2 = frac(x * 37.0 + uTime * speed * 0.71 - y * 0.20);
    band2 = 1.0 - abs(band2 * 2.0 - 1.0);
    band2 = clamp((band2 - 0.62) / 0.38, 0.0, 1.0);
 
    float startCircleX = x / capLength;
    float startCircleY = y;
    float startR = sqrt(startCircleX * startCircleX + startCircleY * startCircleY);
    float startCap = 1.0 - startR;
    startCap = clamp(startCap / 0.06, 0.0, 1.0);
    startCap = startCap * startCap;
 
    float startGlowR = sqrt(startCircleX * startCircleX + startCircleY * startCircleY * 0.35);
    float startGlow = 1.0 - startGlowR;
    startGlow = clamp(startGlow, 0.0, 1.0);
    startGlow = startGlow * startGlow;
 
    float startFlare = 1.0 - abs(y) / 0.055;
    startFlare = clamp(startFlare, 0.0, 1.0);
    startFlare = startFlare * clamp(1.0 - x / (capLength * 2.4), 0.0, 1.0);
 
    float fadeEnd = clamp((1.0 - x) / 0.050, 0.0, 1.0);
 
    float beamMask = max(aura, startGlow);
    beamMask = beamMask * fadeEnd;
 
    float3 color = float3(1.0, 0.98, 0.82) * core * 2.2;
    color = color + uColor * body * (1.0 + band1 * 0.65);
    color = color + uSecondaryColor * aura * (0.55 + band2 * 0.45);
    color = color + float3(1.0, 0.94, 0.75) * startCap * 2.4;
    color = color + uColor * startGlow * 0.95;
    color = color + float3(1.0, 0.50, 0.90) * startFlare * 0.95;
 
    float alpha = core * 1.0 + body * 0.78 + aura * 0.42;
    alpha = max(alpha, startCap * 1.0);
    alpha = max(alpha, startGlow * 0.72);
    alpha = max(alpha, startFlare * 0.65);
    alpha = alpha * beamMask * uOpacity;
    alpha = clamp(alpha, 0.0, 1.0);
 
    return float4(clamp(color, 0.0, 1.0) * alpha, alpha);
}
 
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}