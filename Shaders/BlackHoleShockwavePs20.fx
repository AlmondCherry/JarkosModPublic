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

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 res = uScreenResolution + float2(0.001, 0.001);
    float aspect = res.x / res.y;

    float2 center;
    center.x = (uTargetPosition.x - uScreenPosition.x) / res.x;
    center.y = (uTargetPosition.y - uScreenPosition.y) / res.y;
    center.x = max(0.0, min(1.0, center.x));
    center.y = max(0.0, min(1.0, center.y));

    float progress = uProgress;
    float intensity = uIntensity;
    float opacity = uOpacity;

    float2 p = coords - center;
    p.x = p.x * aspect;

    float dist2 = p.x * p.x + p.y * p.y;
    float radius = 0.040 + progress * 0.450;
    float radius2 = radius * radius;
    float width = 0.030 + progress * 0.060;

    float ring = 1.0 - abs(dist2 - radius2) / width;
    ring = max(0.0, min(1.0, ring));
    ring = ring * ring;

    float fade = 1.0 - progress;
    fade = fade * fade;

    float force = ring * fade * intensity;
    float2 push = coords - center;
    float2 uv = coords + push * force * 0.120;

    float3 color = tex2D(uImage0, uv).rgb;

    float flash = force * opacity * 0.20;
    color.r = color.r + uColor.r * flash;
    color.g = color.g + uColor.g * flash;
    color.b = color.b + uColor.b * flash;

    float keepAlive = uImageOffset.x * 0.000001 + uDirection.x * 0.000001 + uZoom.x * 0.000001 + uTime * 0.0 + uSecondaryColor.x * 0.0;
    color = color + float3(keepAlive, keepAlive, keepAlive);

    return float4(color.r, color.g, color.b, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
