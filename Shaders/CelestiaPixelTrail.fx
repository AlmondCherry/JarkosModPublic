













sampler uImage0 : register(s0);

float  uTime;
float  uOpacity;
float  uPixelSize;
float3 uColorA;
float3 uColorB;
float3 uColorC;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float ps = max(uPixelSize, 0.005);
    float2 uv;
    uv.x = floor(coords.x / ps) * ps + ps * 0.5;
    uv.y = floor(coords.y / ps) * ps + ps * 0.5;

    float x = uv.x;
    float y = uv.y;
    float yc = y - 0.5;

    
    float widthMask = 1.0 - abs(yc) * 2.0;
    widthMask = widthMask * widthMask;

    
    float lenFade = 1.0 - x;
    lenFade = lenFade * lenFade;

    
    float b1 = sin(x * 20.0 - uTime * 6.0 + y * 15.0) * 0.5 + 0.5;
    float b2 = sin(x * 10.0 - uTime * 3.0 - y * 9.0 + 1.7) * 0.5 + 0.5;
    float b = b1 * 0.6 + b2 * 0.4;

    
    float t1 = saturate((b - 0.33) * 3.0);
    float t2 = saturate((b - 0.66) * 3.0);

    float3 col;
    col.x = uColorA.x * (1.0 - t1) + uColorB.x * t1;
    col.y = uColorA.y * (1.0 - t1) + uColorB.y * t1;
    col.z = uColorA.z * (1.0 - t1) + uColorB.z * t1;

    col.x = col.x * (1.0 - t2) + uColorC.x * t2;
    col.y = col.y * (1.0 - t2) + uColorC.y * t2;
    col.z = col.z * (1.0 - t2) + uColorC.z * t2;

    float alphaSrc = (0.4 + b * 0.6) * widthMask * lenFade;
    float alpha = saturate(alphaSrc) * uOpacity;

    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
