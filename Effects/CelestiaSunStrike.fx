sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uProgress;
float uSeed;

float hash21(float2 p)
{
    float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
    float dotv = dot(p3, float3(p3.y + 33.33, p3.z + 33.33, p3.x + 33.33));
    p3 = p3 + dotv;
    return frac((p3.x + p3.y) * p3.z);
}

float valueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + float2(1.0, 0.0));
    float c = hash21(i + float2(0.0, 1.0));
    float d = hash21(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float fbm(float2 p)
{
    float total = valueNoise(p) * 0.5;
    p = p * 2.03; total += valueNoise(p) * 0.25;
    p = p * 2.01; total += valueNoise(p) * 0.125;
    return total * 1.1428;
}

float hash11(float p)
{
    p = frac(p * 0.1031);
    p = p * (p + 33.33);
    return frac(p * p * 2.0);
}

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float along = coords.x;
    float across = coords.y;
    float time = uTime * 30.0 + uSeed;

    float river = fbm(float2(along * 5.0 - time * 0.28, across * 2.0 + uSeed));
    across += (river - 0.5) * 0.18 * (0.35 + uProgress * 0.65);

    float ripple = sin(along * 240.0 - time * 2.4) + sin(along * 570.0 + time * 3.2) * 0.5;
    across += ripple * 0.008 * (0.3 + uProgress * 0.7);

    float dist = abs(across - 0.5) * 2.0;

    float skyFade = smoothstep(0.0, 0.12, along);
    float groundFlare = 1.0 + smoothstep(0.85, 1.0, along) * 1.8 * uProgress;

    float halo = pow(saturate(1.0 - dist), 1.4);

    float coreSize = (0.22 + sin(along * 80.0 - time * 4.0) * 0.05) * (0.35 + uProgress * 0.65);
    float coreOuter = pow(saturate(1.0 - dist / (coreSize * 2.3)), 2.0);
    float coreInner = pow(saturate(1.0 - dist / coreSize), 3.5);

    float tear = fbm(float2(along * 24.0 - time * 0.6, uSeed * 3.7));
    float edgeZone = saturate((dist - 0.45) * 2.2);
    float torn = saturate(tear * 1.6 - 0.35) * edgeZone;

    float seg = floor(along * 34.0 + time * 1.7);
    float segT = frac(along * 34.0 + time * 1.7);
    float b1 = lerp(hash11(seg + uSeed) - 0.5, hash11(seg + 1.0 + uSeed) - 0.5, segT) * 0.45;
    float bolt1 = pow(saturate(1.0 - abs((across - 0.5) - b1 * (0.4 + uProgress * 0.6)) * 12.0), 3.0);
    bolt1 *= step(0.35, hash11(seg + floor(time * 0.8)));

    float seg2 = floor(along * 34.0 - time * 1.3 + 11.0);
    float segT2 = frac(along * 34.0 - time * 1.3 + 11.0);
    float b2 = lerp(hash11(seg2 + uSeed) - 0.5, hash11(seg2 + 1.0 + uSeed) - 0.5, segT2) * -0.45;
    float bolt2 = pow(saturate(1.0 - abs((across - 0.5) - b2 * (0.4 + uProgress * 0.6)) * 12.0), 3.0);
    bolt2 *= step(0.45, hash11(seg2 + floor(time)));

    float bolts = (bolt1 + bolt2 * 0.7) * uProgress;

    float pulse = pow(saturate(sin(along * 14.0 - time * 8.0) * 0.5 + 0.5), 4.0) * uProgress;

    float edgeBand = saturate(1.0 - abs(dist - 0.78) * 5.0);
    float edgeFire = edgeBand * pow(river, 2.0) * uProgress;

    float3 finalColor = lerp(uSecondaryColor, uColor, halo);
    finalColor = lerp(finalColor, uColor * 1.5, coreOuter);
    finalColor = lerp(finalColor, float3(1.0, 1.0, 1.0), coreInner);
    finalColor += float3(0.9, 0.98, 1.0) * bolts * 1.3;
    finalColor += uColor * pulse * halo * 1.7;
    finalColor += uSecondaryColor * edgeFire * 0.9;
    finalColor *= groundFlare;

    float alpha = saturate(halo + bolts * 0.7);
    alpha = saturate(alpha - torn);
    alpha *= skyFade * uOpacity;

    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass SunStrikePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
