sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float Hash21(float2 value)
{
    value = frac(value * float2(123.34, 456.21));
    value += dot(value, value + 45.32);
    return frac(value.x * value.y);
}

float Noise(float2 value)
{
    float2 cell = floor(value);
    float2 local = frac(value);
    float2 smoothLocal = local * local * (3.0 - 2.0 * local);

    float bottomLeft = Hash21(cell);
    float bottomRight = Hash21(cell + float2(1.0, 0.0));
    float topLeft = Hash21(cell + float2(0.0, 1.0));
    float topRight = Hash21(cell + float2(1.0, 1.0));

    float bottom = lerp(bottomLeft, bottomRight, smoothLocal.x);
    float top = lerp(topLeft, topRight, smoothLocal.x);
    return lerp(bottom, top, smoothLocal.y);
}

float StarLayer(float2 coords, float density, float size, float cutoff, float time)
{
    float2 grid = coords * density;
    float2 cell = floor(grid);
    float2 local = frac(grid) - 0.5;

    float random = Hash21(cell);
    float sparkle = step(cutoff, random);
    float twinkle = 0.55 + 0.45 * sin(time * (2.4 + random * 4.8) + random * 31.0);
    float distanceToStar = length(local);
    float star = pow(Clamp01(1.0 - distanceToStar / size), 8.0);

    return sparkle * star * twinkle;
}

float BigStar(float2 coords, float2 center, float radius, float time, float phase)
{
    float2 delta = coords - center;
    float distanceToCenter = length(delta);
    float core = pow(Clamp01(1.0 - distanceToCenter / radius), 14.0);
    float glow = pow(Clamp01(1.0 - distanceToCenter / (radius * 4.5)), 3.0);
    float cross = pow(Clamp01(1.0 - abs(delta.x) / (radius * 0.22)), 8.0) * pow(Clamp01(1.0 - abs(delta.y) / (radius * 3.8)), 2.0);
    cross += pow(Clamp01(1.0 - abs(delta.y) / (radius * 0.22)), 8.0) * pow(Clamp01(1.0 - abs(delta.x) / (radius * 3.8)), 2.0);
    float twinkle = 0.72 + 0.28 * sin(time * 5.0 + phase);

    return (core + glow * 0.4 + cross * 0.2) * twinkle;
}

float AuroraBand(float2 coords, float yOffset, float width, float waveScale, float speed, float time)
{
    float waveA = sin(coords.x * waveScale + time * speed);
    float waveB = sin(coords.x * (waveScale * 0.57) - time * (speed * 0.63) + 2.1);
    float center = yOffset + waveA * 0.055 + waveB * 0.04;
    float band = 1.0 - abs(coords.y - center) / width;
    band = Clamp01(band);
    return band * band * (3.0 - 2.0 * band);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float4 mask = tex2D(uImage0, coords);
    float alpha = mask.a * vertexColor.a * Clamp01(uOpacity);

    if (alpha <= 0.001)
        return float4(0.0, 0.0, 0.0, 0.0);

    float time = uSaturation;
    float2 starDrift = float2(time * 0.018, -time * 0.011);
    float2 nebulaDrift = float2(time * 0.055, time * 0.019);
    float2 nebulaUv = coords * 3.15 + nebulaDrift;

    float nebulaA = Noise(nebulaUv);
    float nebulaB = Noise(nebulaUv * 2.35 + float2(7.4, 2.9));
    float nebulaC = Noise(nebulaUv * 5.1 - float2(3.2, 8.7));
    float nebula = Clamp01((nebulaA * 0.55 + nebulaB * 0.5 + nebulaC * 0.22 - 0.36) * 1.55);

    float stars = StarLayer(coords + starDrift, 46.0, 0.25, 0.952, time) * 0.85;
    stars += StarLayer(coords - starDrift * 0.7 + float2(0.17, 0.41), 82.0, 0.29, 0.968, time) * 1.2;
    stars += StarLayer(coords + starDrift * 1.8 + float2(0.53, 0.09), 142.0, 0.33, 0.982, time) * 0.8;
    stars += BigStar(coords + starDrift * 0.5, float2(0.24, 0.34), 0.012, time, 1.7) * 0.95;
    stars += BigStar(coords - starDrift * 0.35, float2(0.72, 0.61), 0.009, time, 5.4) * 0.75;

    float auroraA = AuroraBand(coords + float2(time * 0.035, 0.0), 0.28, 0.16, 9.0, 1.55, time);
    float auroraB = AuroraBand(coords - float2(time * 0.02, 0.0), 0.55, 0.19, 7.0, 1.1, time + 4.0);
    float aurora = Clamp01(auroraA * 0.85 + auroraB * 0.6) * (0.55 + nebulaB * 0.5);

    float verticalFade = smoothstep(0.0, 0.2, coords.y) * smoothstep(1.0, 0.72, coords.y);
    float3 voidBlack = float3(0.004, 0.003, 0.012);
    float3 deepPurple = float3(0.21, 0.03, 0.36);
    float3 hotViolet = float3(0.58, 0.11, 0.92);
    float3 softBlue = float3(0.16, 0.58, 1.0);
    float3 nebulaColor = lerp(deepPurple, uSecondaryColor, nebulaB);
    float3 auroraColor = lerp(hotViolet, softBlue, Clamp01(nebulaC * 0.7 + coords.y * 0.3));
    float3 color = voidBlack;

    color += nebulaColor * nebula * 0.28 * verticalFade;
    color += auroraColor * aurora * 0.42 * verticalFade;
    color += float3(0.74, 0.9, 1.0) * stars;
    color += uColor * nebula * aurora * 0.08;
    color = min(color, float3(1.0, 1.0, 1.0));

    return float4(color, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
