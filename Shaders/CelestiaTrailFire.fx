











sampler uImage0 : register(s0);

float  uTime;
float  uOpacity;
float  uIntensity;
float  uPixelSize;
float3 uHotColor;
float3 uMidColor;
float3 uCoolColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 uv = coords;
    if (uPixelSize > 0.0001)
    {
        uv.x = floor(uv.x / uPixelSize) * uPixelSize + uPixelSize * 0.5;
        uv.y = floor(uv.y / uPixelSize) * uPixelSize + uPixelSize * 0.5;
    }

    float x = uv.x;
    float y = uv.y;
    float yc = y - 0.5;

    
    float widthMask = 1.0 - abs(yc) * 2.0;
    widthMask = widthMask * widthMask;

    
    float lenFade = 1.0 - x;
    lenFade = lenFade * lenFade;

    
    float t1 = sin(x * 25.0 - uTime * 6.0 + yc * 9.0) * 0.5 + 0.5;
    float t2 = sin(x * 12.0 - uTime * 4.0 + yc * 6.0 + 1.7) * 0.5 + 0.5;
    float fire = t1 * 0.6 + t2 * 0.4;
    fire = fire * widthMask;

    
    float spx = sin(x * 70.0 - uTime * 8.0);
    float spy = sin(y * 30.0 + uTime * 5.0);
    float sparkle = saturate(spx * spy - 0.85) * 6.5;

    
    float coreMask = 1.0 - abs(yc) * 4.0;
    coreMask = saturate(coreMask);
    coreMask = coreMask * coreMask * coreMask;

    
    float fireAmt = saturate(fire * uIntensity + 0.15);

    float3 col;
    col.x = uCoolColor.x * (1.0 - fireAmt) + uMidColor.x * fireAmt;
    col.y = uCoolColor.y * (1.0 - fireAmt) + uMidColor.y * fireAmt;
    col.z = uCoolColor.z * (1.0 - fireAmt) + uMidColor.z * fireAmt;

    col.x = col.x + uHotColor.x * coreMask * 0.85 + sparkle;
    col.y = col.y + uHotColor.y * coreMask * 0.85 + sparkle;
    col.z = col.z + uHotColor.z * coreMask * 0.85 + sparkle;

    
    float alphaSrc = fire * 0.7 + coreMask * 0.6 + sparkle * 0.5;
    float alpha = saturate(alphaSrc * widthMask * lenFade) * uOpacity;

    
    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
