



sampler uImage0 : register(s0);

float uTime;
float uSeed;
float3 uColor;
float uUnstable;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 rel = coords - float2(0.5, 0.5);
    float r = sqrt(rel.x * rel.x + rel.y * rel.y) * 2.0;

    float n = tex2D(uImage0, float2(coords.x * 1.4 + uSeed + uTime * 0.05, coords.y * 1.4 + uSeed * 0.7 - uTime * 0.03)).r;
    float n2 = tex2D(uImage0, float2(coords.x * 3.0 - uTime * 0.07, coords.y * 3.0 + uSeed + uTime * 0.04)).g;

    
    float edge = r + (n - 0.5) * 0.16;
    float body = saturate((0.86 - edge) * 9.0);

    
    float rim = body * saturate((edge - 0.58) * 5.0);

    
    float bub = saturate((n2 - 0.55) * 6.0) * saturate((0.72 - edge) * 6.0);

    
    float puls = 0.85 + 0.15 * sin(uTime * 3.5 + uSeed * 40.0);
    float nuc = saturate((0.30 * puls - r) * 6.0);

    
    float lightK = saturate(0.55 - rel.x * 0.7 - rel.y * 0.9);
    float3 inter = uColor * (0.28 + 0.38 * lightK + 0.22 * n);

    
    float fl = 1.0 + uUnstable * (0.4 + 0.4 * sin(uTime * 17.0 + uSeed * 60.0));

    float3 col = inter * body
               + uColor * bub * 0.55
               + uColor * rim * 1.35
               + float3(1.0, 1.0, 0.9) * rim * rim * 0.30
               + (uColor * 0.6 + float3(0.5, 0.6, 0.4)) * nuc;
    col = col * fl;

    float a = saturate(body + rim * 0.5);
    return float4(col * a, a);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
