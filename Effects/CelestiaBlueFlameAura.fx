sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float uTime;
float uIntensity;

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

    float a = hash21(i + float2(0.0, 0.0));
    float b = hash21(i + float2(1.0, 0.0));
    float c = hash21(i + float2(0.0, 1.0));
    float d = hash21(i + float2(1.0, 1.0));

    float ab = lerp(a, b, u.x);
    float cd = lerp(c, d, u.x);
    return lerp(ab, cd, u.y);
}

float fbm(float2 p)
{
    float total = 0.0;
    float amp = 0.5;

    [unroll]
    for (int oct = 0; oct < 4; oct++)
    {
        total += valueNoise(p) * amp;
        p = p * 2.0;
        amp = amp * 0.5;
    }
    return total;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 dir = coords - center;

    float distSq = dot(dir, dir);
    float dist = sqrt(distSq);

    float angle = atan2(dir.y, dir.x);

    float speed = 0.6 + uIntensity * 1.4;
    float flow = uTime * speed;

    float swirl = angle * 1.5 + dist * 6.0;
    float2 np;
    np.x = swirl + flow * 0.5;
    np.y = dist * 8.0 - flow;

    float2 np2;
    np2.x = swirl * 0.7 - flow * 0.3;
    np2.y = dist * 5.0 - flow * 1.3;

    float n1 = fbm(np);
    float n2 = fbm(np2);
    float turb = n1 * 0.65 + n2 * 0.35;

    float flameReach = 0.35 + uIntensity * 0.30;
    float edge = dist / flameReach;

    float burn = edge + (turb - 0.5) * (0.9 + uIntensity * 0.8);

    float flame = 1.0 - burn;
    flame = saturate(flame);

    float flameSharp = flame * flame;

    float pulse = 0.85 + 0.15 * sin(uTime * (2.0 + uIntensity * 3.0));
    flameSharp = flameSharp * pulse;

    float coreFalloff = 1.0 - dist * (2.2 - uIntensity * 0.6);
    coreFalloff = saturate(coreFalloff);
    float core = coreFalloff * coreFalloff;
    core = core * core;
    core = core * (1.0 + uIntensity * 1.5);

    float3 colEdge = float3(0.00, 0.00, 0.55);
    float3 colBody = float3(0.10, 0.55, 1.00);
    float3 colCore = float3(0.88, 1.00, 1.00);

    float bodyMix = saturate(flameSharp * 1.3);
    float3 fireColor = lerp(colEdge, colBody, bodyMix);

    fireColor = lerp(fireColor, colCore, core);

    float hotSpots = saturate(turb * flameSharp * 1.5);
    fireColor = fireColor + colBody * hotSpots * (0.4 + uIntensity * 0.9);

    float bloom = 1.0 + uIntensity * 1.2;
    fireColor = fireColor * bloom;

    float alpha = saturate(flameSharp + core);

    float2 sampleCoords = coords;
    float4 mask = tex2D(uImage0, sampleCoords);

    alpha = alpha * mask.a;

    float4 result;
    result.rgb = fireColor * alpha;
    result.a = alpha;

    return result * sampleColor;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}