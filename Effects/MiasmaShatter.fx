sampler uImage0 : register(s0);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uProgress;
float uTime;
float uIntensity;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float aspect = uScreenResolution.x / uScreenResolution.y;

    float2 center;
    center.x = (uTargetPosition.x - uScreenPosition.x) / uScreenResolution.x;
    center.y = (uTargetPosition.y - uScreenPosition.y) / uScreenResolution.y;

    float2 p;
    p.x = (coords.x - center.x) * aspect;
    p.y = coords.y - center.y;

    float crackT = saturate(uProgress * 5.0);
    float flyT = saturate((uProgress - 0.2) * 1.54);
    float fly = flyT * flyT;
    float endFade = saturate((uProgress - 0.85) * 6.667);

    float d1 = p.x * 0.309 + p.y * 0.951 + 0.030;
    float d2 = p.x * -0.809 + p.y * 0.588 - 0.055;
    float d3 = p.x * 0.914 + p.y * -0.406 + 0.020;
    float d4 = p.x * 0.559 + p.y * 0.829 - 0.130;

    float s1 = step(0.0, d1) * 2.0 - 1.0;
    float s2 = step(0.0, d2) * 2.0 - 1.0;
    float s3 = step(0.0, d3) * 2.0 - 1.0;
    float s4 = step(0.0, d4) * 2.0 - 1.0;

    float2 off;
    off.x = s1 * 0.309 * 0.11 + s2 * -0.809 * 0.15 + s3 * 0.914 * 0.12 + s4 * 0.559 * 0.17;
    off.y = s1 * 0.951 * 0.11 + s2 * 0.588 * 0.15 + s3 * -0.406 * 0.12 + s4 * 0.829 * 0.17;

    float tumble = s1 * 0.5 + s2 * 0.3 - s3 * 0.4 + s4 * 0.2;
    off.x = off.x - p.y * tumble * 0.06 * flyT;
    off.y = off.y + p.x * tumble * 0.06 * flyT;
    off = off * fly;

    float ad1 = abs(d1);
    float ad2 = abs(d2);
    float ad3 = abs(d3);
    float ad4 = abs(d4);
    float minAd = min(min(ad1, ad2), min(ad3, ad4));
    float edgeNear = saturate(1.0 - minAd * 16.0);
    float wob = sin(p.x * 15.0 + p.y * 21.0 + uTime * 3.0);
    float2 refr;
    refr.x = wob * edgeNear * 0.014 * crackT;
    refr.y = wob * edgeNear * -0.011 * crackT;

    float2 duv;
    duv.x = (off.x + refr.x) / aspect;
    duv.y = off.y + refr.y;
    float2 sc = coords + duv;
    sc = lerp(sc, coords, endFade);
    sc = clamp(sc, 0.002, 0.998);

    float4 col = tex2D(uImage0, sc);

    float band = sin((p.x + p.y) * 6.0 + tumble * 4.0 + uTime * 1.5 + fly * 9.0);
    band = saturate(band);
    band = band * band;
    float sheen = band * flyT * (1.0 - endFade) * 0.4;
    col.r = col.r + sheen * 0.45;
    col.g = col.g + sheen * 0.55;
    col.b = col.b + sheen * 0.95;

    float sparkle = 0.6 + 0.4 * sin(uTime * 10.0 + p.x * 40.0 + p.y * 33.0);
    float glow = edgeNear * crackT * (1.0 - endFade) * sparkle * (1.3 - fly * 0.6);
    col.r = col.r + glow;
    col.g = col.g + glow;
    col.b = col.b + glow;

    col.r = col.r + uIntensity * 0.9;
    col.g = col.g + uIntensity * 0.85;
    col.b = col.b + uIntensity;

    return col * sampleColor;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}