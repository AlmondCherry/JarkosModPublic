sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageOffset;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uZoom;

float Clamp01(float value)
{
    return max(0.0, min(1.0, value));
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 res = uScreenResolution + float2(0.001, 0.001);
    float aspect = res.x / res.y;

    float2 center;
    center.x = (uTargetPosition.x - uScreenPosition.x) / res.x;
    center.y = (uTargetPosition.y - uScreenPosition.y) / res.y;
    center.x = Clamp01(center.x);
    center.y = Clamp01(center.y);

    float2 p = coords - center;
    p.x *= aspect;

    float r = sqrt(dot(p, p) + 0.0001);
    float2 radial = p / (r + 0.0001);

    float progress = Clamp01(uProgress);
    float intensity = Clamp01(uIntensity);
    float opacity = Clamp01(uOpacity);

    float radius = 0.035 + progress * 0.46;
    float width = 0.070 * (1.0 - progress) + 0.018;

    float ring = 1.0 - Clamp01(abs(r - radius) / width);
    ring = ring * ring;

    float fade = 1.0 - progress;
    fade = fade * fade;

    float ripple = sin((r - radius) * 95.0 - uTime * 8.0) * 0.5 + 0.5;
    float force = ring * fade * intensity;

    float2 distortion = radial * force * (0.030 + ripple * 0.010);
    float2 tangent = float2(-radial.y, radial.x);
    distortion += tangent * force * 0.010;

    float2 uv = coords + distortion;
    uv.x = Clamp01(uv.x);
    uv.y = Clamp01(uv.y);

    float chroma = force * 0.006;
    float2 uvR = uv + float2(chroma, 0.0);
    float2 uvB = uv - float2(chroma, 0.0);
    uvR.x = Clamp01(uvR.x);
    uvB.x = Clamp01(uvB.x);

    float3 original = tex2D(uImage0, coords).rgb;

    float red = tex2D(uImage0, uvR).r;
    float green = tex2D(uImage0, uv).g;
    float blue = tex2D(uImage0, uvB).b;
    float3 col = float3(red, green, blue);

    float flash = ring * fade * intensity;
    col += uColor * flash * 0.22;
    col += uSecondaryColor * flash * ripple * 0.10;

    float centerPulse = 1.0 - Clamp01(r / 0.12);
    centerPulse = centerPulse * centerPulse * fade;
    col += uColor * centerPulse * intensity * 0.10;

    float keepAlive = 0.0;
    keepAlive += uImageOffset.x * 0.000001 + uDirection.x * 0.000001 + uZoom.x * 0.000001;
    col += float3(keepAlive, keepAlive, keepAlive);

    return float4(lerp(original, col, opacity), 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}