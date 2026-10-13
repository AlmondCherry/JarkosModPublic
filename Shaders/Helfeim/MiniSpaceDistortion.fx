sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uGlobalOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;
float uSaturation;
float4 uSourceRect;

float2 uPortalPositions[6];
float uPortalFrames[6];
float uPortalPowers[6];
float uPortalCount;
float uPortalDuration;
float2 uPortalScreenPosition;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float Smooth01(float value)
{
    value = Clamp01(value);
    return value * value * (3.0 - 2.0 * value);
}

float2 ClampUv(float2 value)
{
    return float2(Clamp01(value.x), Clamp01(value.y));
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;
    float2 zoom = max(uDirection, float2(0.001, 0.001));
    float opacity = Clamp01(max(uOpacity, uGlobalOpacity));
    float duration = max(uPortalDuration, 1.0);

    float2 totalWarp = float2(0.0, 0.0);
    float totalPortal = 0.0;
    float totalRing = 0.0;
    float totalCore = 0.0;

    for (int i = 0; i < 6; i++)
    {
        float frame = uPortalFrames[i];
        if (frame <= 0.5)
            continue;

        float life = Clamp01(1.0 - frame / duration);
        float power = Clamp01(uPortalPowers[i]);
        float2 center = ((uPortalPositions[i] - uPortalScreenPosition) * zoom) / screenSize;
        float2 p = coords - center;
        p.x *= aspect;

        float dist = length(p);
        float2 radialUv = p / max(dist, 0.0001);
        radialUv.x /= aspect;

        float portalFade = Smooth01(life / 0.20) * (1.0 - Smooth01((life - 0.68) / 0.32)) * power;
        float radius = 0.052 + life * 0.070;
        float core = Clamp01(1.0 - dist / radius);
        core *= core * portalFade;

        float portal = Clamp01(1.0 - dist / (radius + 0.11));
        portal *= portal * portalFade;

        float ring = Clamp01(1.0 - abs(dist - radius) / 0.055) * portalFade;
        totalWarp += radialUv * (ring * 0.009 + portal * 0.0035 + core * 0.0015);
        totalPortal += portal;
        totalRing += ring;
        totalCore += core;
    }

    float strength = Clamp01(totalPortal + totalRing + totalCore);
    if (strength <= 0.001)
        return tex2D(uImage0, coords);

    float2 uv = ClampUv(coords + totalWarp * opacity);
    float3 baseColor = tex2D(uImage0, coords).rgb;
    float3 warped = tex2D(uImage0, uv).rgb;

    float2 chromaDir = normalize(totalWarp + float2(0.0001, 0.0));
    float chroma = Clamp01(totalRing + totalPortal * 0.25) * 0.0028 * opacity;
    float red = tex2D(uImage0, ClampUv(uv + chromaDir * chroma)).r;
    float blue = tex2D(uImage0, ClampUv(uv - chromaDir * chroma)).b;

    float3 color = lerp(baseColor, warped, Clamp01(totalRing + totalPortal * 0.3));
    color.r = lerp(color.r, red, 0.24);
    color.b = lerp(color.b, blue, 0.24);
    color = lerp(color, uSecondaryColor, Clamp01(totalPortal) * 0.16 * opacity);
    color += uColor * Clamp01(totalCore) * 0.06 * opacity;
    color = min(color, float3(1.0, 1.0, 1.0));

    float keepAlive = zoom.x * 0.000001 + zoom.y * 0.000001 + uPortalCount * 0.0000001;
    color.rgb += float3(keepAlive, keepAlive, keepAlive);

    return float4(color, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
