






sampler uImage0 : register(s0);
float  uTime;
float  uProgress;
float  uOpacity;
float  uPixelMix;
float3 uBaseColor;
float3 uAccentColor;
float3 uGlowColor;
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 texColor = tex2D(uImage0, coords);
    
    
    
    float dx = (coords.x - 0.5) * 7.0;
    float dy = coords.y - 0.0;
    float dist = sqrt(dx * dx + dy * dy);
    
    
    float ringPhase1 = dist * 5.5  - uTime * 1.6;
    float ringPhase2 = dist * 9.0  - uTime * 2.4 + 1.7;
    float ringPhase3 = dist * 14.0 - uTime * 3.5 + 3.1;
    float r1 = sin(ringPhase1) * 0.5 + 0.5;
    float r2 = sin(ringPhase2) * 0.5 + 0.5;
    float r3 = sin(ringPhase3) * 0.5 + 0.5;
    
    float ringSoft = r1 * r1 * 0.55 + r2 * r2 * 0.30 + r3 * r3 * 0.15;
    float ringSharp = r2 * r2 * r2 * r2;
    
    float distFade = saturate(1.0 - dist * 0.55);
    
    
    float stripeY = coords.y * 60.0 - uTime * 4.5;
    float stripe = sin(stripeY) * 0.5 + 0.5;
    stripe = stripe * stripe * stripe;
    
    float hueShift = sin(uTime * 1.2 + coords.y * 3.0) * 0.5 + 0.5;
    
    float sideMask = abs(coords.x - 0.5) * 2.0; 
    float centerMask = 1.0 - sideMask;
    
    float heartbeat = sin(uTime * 4.5 + uProgress * 5.0) * 0.5 + 0.5;
    heartbeat = 0.55 + heartbeat * 0.45;
    
    
    float3 baseLayer;
    baseLayer.x = uBaseColor.x * (0.55 + hueShift * 0.45) + uAccentColor.x * 0.20;
    baseLayer.y = uBaseColor.y * (0.55 + hueShift * 0.45) + uAccentColor.y * 0.20;
    baseLayer.z = uBaseColor.z * (0.55 + hueShift * 0.45) + uAccentColor.z * 0.20;
    
    float ringPower = ringSoft * distFade;
    float3 pulseLayer;
    pulseLayer.x = uAccentColor.x * ringSharp * distFade * 1.2 + uGlowColor.x * stripe * 0.45;
    pulseLayer.y = uAccentColor.y * ringSharp * distFade * 1.2 + uGlowColor.y * stripe * 0.45;
    pulseLayer.z = uAccentColor.z * ringSharp * distFade * 1.2 + uGlowColor.z * stripe * 0.45;
    float3 final;
    final.x = baseLayer.x + pulseLayer.x;
    final.y = baseLayer.y + pulseLayer.y;
    final.z = baseLayer.z + pulseLayer.z;
    final.x = final.x * heartbeat;
    final.y = final.y * heartbeat;
    final.z = final.z * heartbeat;
    
    
    float alphaCore = 0.18 + sideMask * 0.32 + ringPower * 0.40 + ringSharp * distFade * 0.35;
    alphaCore = alphaCore + centerMask * stripe * 0.10;
    float alpha = alphaCore * uOpacity * uProgress * texColor.a;
    return float4(final, alpha);
}
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}