sampler uImage0 : register(s0);

float uTime;
float uIntensity;
float2 uScreenResolution;

float hash31(float3 p)
{
    float3 p3 = frac(p * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float noise3(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * (3.0 - 2.0 * f);

    float n000 = hash31(i);
    float n100 = hash31(i + float3(1, 0, 0));
    float n010 = hash31(i + float3(0, 1, 0));
    float n110 = hash31(i + float3(1, 1, 0));
    float n001 = hash31(i + float3(0, 0, 1));
    float n101 = hash31(i + float3(1, 0, 1));
    float n011 = hash31(i + float3(0, 1, 1));
    float n111 = hash31(i + float3(1, 1, 1));

    float nx00 = lerp(n000, n100, u.x);
    float nx10 = lerp(n010, n110, u.x);
    float nx01 = lerp(n001, n101, u.x);
    float nx11 = lerp(n011, n111, u.x);

    return lerp(lerp(nx00, nx10, u.y), lerp(nx01, nx11, u.y), u.z);
}

float fbm3(float3 p)
{
    float total = noise3(p) * 0.5;
    p = p * 2.03; total += noise3(p) * 0.25;
    p = p * 2.01; total += noise3(p) * 0.125;
    return total * 1.1428;
}

float cloudDensity(float3 p, float t)
{
    float3 wind = float3(t * 0.035, 0.0, t * 0.012);
    float3 q = p * 0.28 + wind;
    float f = fbm3(q);
    float hMask = smoothstep(0.9, 1.6, p.y) * (1.0 - smoothstep(2.6, 3.6, p.y));
    return saturate((f - 0.44) * 2.4) * hMask;
}

void marchStep(float3 pos, float3 sunDir, float t, float dt, float3 litCol, float3 shadCol, inout float3 acc, inout float trans)
{
    float den = cloudDensity(pos, t);
    float denL = cloudDensity(pos + sunDir * 0.35, t);
    float lit = saturate((den - denL) * 5.0 + 0.55);
    float powder = 1.0 - exp(-den * 5.0);
    float3 c = lerp(shadCol, litCol, lit) * (0.55 + powder * 0.45);
    float a = saturate(den * dt * 1.4);
    acc += c * a * trans;
    trans *= 1.0 - a;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float aspect = uScreenResolution.x / uScreenResolution.y;
    float t = uTime;

    float3 rd = normalize(float3((coords.x - 0.5) * aspect, (0.62 - coords.y) * 1.1, 1.0));

    float3 sunDir = normalize(float3(0.45, 0.38, 0.55));
    float sunDot = saturate(dot(rd, sunDir));
    float3 sunColor = float3(1.0, 0.95, 0.85);

    float up = saturate(rd.y);
    float3 zenith  = float3(0.05, 0.15, 0.40);
    float3 horizon = float3(0.55, 0.78, 0.98);
    float3 sky = lerp(horizon, zenith, pow(up, 0.6));

    sky = lerp(sky, float3(0.35, 0.50, 0.75), saturate(-rd.y * 3.0));

    sky += sunColor * pow(sunDot, 350.0) * 3.0;
    sky += sunColor * pow(sunDot, 8.0) * 0.35;

    float2 starUV = float2(coords.x * aspect, coords.y) * 70.0;
    float sRnd = hash31(float3(floor(starUV), 7.0));
    float2 sLoc = frac(starUV) - 0.5;
    float star = saturate(1.0 - dot(sLoc, sLoc) * 20.0);
    star = star * star * saturate((sRnd - 0.97) * 40.0);
    sky += float3(0.9, 0.95, 1.0) * star * (0.6 + 0.4 * sin(t * 3.0 + sRnd * 40.0)) * up;

    float3 finalColor = sky;
    if (rd.y > 0.015)
    {
        float t1 = 1.0 / rd.y;
        float t2 = min(3.4 / rd.y, t1 + 14.0);
        float dt = (t2 - t1) / 5.0;

        float3 litCol  = float3(1.05, 1.02, 0.98) + sunColor * pow(sunDot, 6.0) * 0.6;
        float3 shadCol = float3(0.24, 0.33, 0.55);

        float3 acc = float3(0.0, 0.0, 0.0);
        float trans = 1.0;
        float tc = t1 + dt * 0.5;

        marchStep(rd * tc, sunDir, t, dt, litCol, shadCol, acc, trans); tc += dt;
        marchStep(rd * tc, sunDir, t, dt, litCol, shadCol, acc, trans); tc += dt;
        marchStep(rd * tc, sunDir, t, dt, litCol, shadCol, acc, trans); tc += dt;
        marchStep(rd * tc, sunDir, t, dt, litCol, shadCol, acc, trans); tc += dt;
        marchStep(rd * tc, sunDir, t, dt, litCol, shadCol, acc, trans);

        float horizFade = smoothstep(0.015, 0.10, rd.y);
        float3 withClouds = acc + sky * trans;
        finalColor = lerp(sky, withClouds, horizFade);
    }

    finalColor = saturate(finalColor);

    float a = saturate(uIntensity);
    return float4(finalColor * a, a) * sampleColor;
}

technique Technique1
{
    pass CelestiaSkyPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
