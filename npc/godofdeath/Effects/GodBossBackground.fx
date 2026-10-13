float2 uResolution;
float iTime;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 resolution = max(uResolution, float2(1.0, 1.0));
    float2 fragCoord = coords * resolution;
    float2 uv = (2.0 * fragCoord - resolution) / min(resolution.x, resolution.y);

    
    float slowTime = iTime * 0.5;

    [unroll]
    for (int step = 1; step < 10; step++)
    {
        float i = (float)step;
        uv.x += 0.6 / i * cos(i * 2.5 * uv.y + slowTime);
        uv.y += 0.6 / i * cos(i * 1.5 * uv.x + slowTime);
    }

    float wave = abs(sin(slowTime - uv.y - uv.x));
    float value = saturate(0.1 / max(wave, 0.035));
    
    
    float3 color = value.xxx * 0.4;

    return float4(color * uOpacity, uOpacity * 0.4);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
