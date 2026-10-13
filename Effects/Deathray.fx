sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uProgress;

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
    float time = uTime * 30.0;

    float wave1 = sin(along * 300.0 - time * 2.0);
    float wave2 = sin(along * 700.0 + time * 3.0) * 0.5;
    float wave3 = sin(along * 90.0  - time * 1.2) * 0.7;
    across += (wave1 + wave2 + wave3) * 0.012 * (0.4 + uProgress * 0.6);

    float dist = abs(across - 0.5) * 2.0;

    float taper = smoothstep(0.0, 0.002, along);
    float endFade = 1.0 - smoothstep(0.85, 1.0, along) * 0.5;

    float halo = pow(saturate(1.0 - dist), 1.5);

    float coreSize = (0.16 + sin(along * 100.0 - time * 4.0) * 0.04) * (0.35 + uProgress * 0.65);
    float coreOuter = pow(saturate(1.0 - dist / (coreSize * 2.2)), 2.0);
    float coreInner = pow(saturate(1.0 - dist / coreSize), 4.0);

    float seg = floor(along * 40.0 + time * 1.5);
    float segT = frac(along * 40.0 + time * 1.5);
    float b1a = (hash11(seg) - 0.5) * 0.45;
    float b1b = (hash11(seg + 1.0) - 0.5) * 0.45;
    float boltY1 = lerp(b1a, b1b, segT);
    float bolt1 = pow(saturate(1.0 - abs((across - 0.5) - boltY1 * (0.5 + uProgress * 0.5)) * 14.0), 3.0);
    bolt1 *= step(0.35, hash11(seg + floor(time * 0.7)));

    float seg2 = floor(along * 40.0 - time * 1.2 + 17.0);
    float segT2 = frac(along * 40.0 - time * 1.2 + 17.0);
    float b2a = (hash11(seg2) - 0.5) * -0.45;
    float b2b = (hash11(seg2 + 1.0) - 0.5) * -0.45;
    float boltY2 = lerp(b2a, b2b, segT2);
    float bolt2 = pow(saturate(1.0 - abs((across - 0.5) - boltY2 * (0.5 + uProgress * 0.5)) * 14.0), 3.0);
    bolt2 *= step(0.45, hash11(seg2 + floor(time * 0.9)));

    float bolts = (bolt1 + bolt2 * 0.7) * uProgress;

    float pulseWave = pow(saturate(sin(along * 20.0 - time * 6.0) * 0.5 + 0.5), 4.0) * uProgress;

    float stream = pow(sin(along * 150.0 - time * 2.5) * 0.5 + 0.5, 3.0);
    float edgeBand = saturate(1.0 - abs(dist - 0.75) * 5.0);
    float flame = pow(sin(along * 60.0 - time * 3.5) * 0.5 + 0.5, 2.0);
    float edgeFire = edgeBand * flame * uProgress;

    float3 finalColor = lerp(uSecondaryColor, uColor, halo);
    finalColor = lerp(finalColor, uColor * 1.4, coreOuter);
    finalColor = lerp(finalColor, float3(1.0, 1.0, 1.0), coreInner);
    finalColor += float3(1.0, 0.9, 1.0) * bolts * 1.2;
    finalColor += uColor * (stream + pulseWave) * halo * 1.5;
    finalColor += uSecondaryColor * edgeFire * 0.8;

    float alpha = saturate(halo + bolts * 0.7) * taper * endFade * uOpacity;

    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass DeathrayPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
