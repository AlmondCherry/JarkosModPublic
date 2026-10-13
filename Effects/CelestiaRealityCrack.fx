sampler uImage0 : register(s0);

float uTime;
float uSeed;
float uProgress;
float uOpacity;

float hash11(float p)
{
    p = frac(p * 0.1031);
    p = p * (p + 33.33);
    return frac(p * p * 2.0);
}

float crackRay(float angle01, float dist, float idx, float seed, float reach)
{
    float rayAng = hash11(idx * 17.0 + seed);
    float d = angle01 - rayAng;
    d = d - floor(d + 0.5);

    float seg = floor(dist * 10.0 + idx * 3.0);
    float jag = (hash11(seg + seed + idx * 7.0) - 0.5) * 0.14;

    float thick = 26.0 + hash11(idx + seed) * 14.0;
    float ray = pow(saturate(1.0 - abs(d + jag) * thick), 2.5);

    ray *= smoothstep(0.0, 0.05, dist) * (1.0 - smoothstep(reach * 0.7, reach, dist));
    return ray;
}

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 dir = coords - float2(0.5, 0.5);
    float dist = sqrt(dot(dir, dir)) * 2.0;
    float angle01 = atan2(dir.y, dir.x) / 6.2831853 + 0.5;

    float pulse = 0.85 + 0.15 * sin(uTime * 10.0 + dist * 8.0);
    float reach = uProgress;

    float c = crackRay(angle01, dist, 1.0, uSeed, reach);
    c += crackRay(angle01, dist, 2.0, uSeed, reach * 0.9);
    c += crackRay(angle01, dist, 3.0, uSeed, reach);
    c += crackRay(angle01, dist, 4.0, uSeed, reach * 0.8);
    c += crackRay(angle01, dist, 5.0, uSeed, reach);
    c += crackRay(angle01, dist, 6.0, uSeed, reach * 0.85);
    c += crackRay(angle01, dist, 7.0, uSeed, reach * 0.95);
    c += crackRay(angle01, dist, 8.0, uSeed, reach * 0.75);
    c += crackRay(angle01, dist, 9.0, uSeed, reach);
    c += crackRay(angle01, dist, 10.0, uSeed, reach * 0.9);
    c = saturate(c) * pulse;

    float coreGlow = pow(saturate(1.0 - dist * 2.5), 2.0) * uProgress;

    float3 violet = float3(0.60, 0.20, 1.00);
    float3 white  = float3(1.00, 0.97, 1.00);

    float3 finalColor = violet * c * 1.2 + white * pow(c, 3.0) * 1.5;
    finalColor += violet * coreGlow;

    float alpha = saturate(c + coreGlow * 0.6) * uOpacity;
    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass RealityCrackPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
