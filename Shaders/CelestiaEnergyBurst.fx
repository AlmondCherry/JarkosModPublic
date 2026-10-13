sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uProgress;
float uIntensity;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float Smooth01(float value)
{
    value = Clamp01(value);
    return value * value * (3.0 - 2.0 * value);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;

    float2 center = (uTargetPosition - uScreenPosition) / screenSize;
    float2 p = coords - center;
    p.x *= aspect;

    float dist = length(p);
    float2 radial = p / max(dist, 0.0001);
    float2 radialUv = float2(radial.x / aspect, radial.y);

    float progress = Clamp01(uProgress);
    float opacity = Clamp01(uOpacity);
    float power = Clamp01(uIntensity);
    float fade = 1.0 - progress;

    float grow = Smooth01(progress / 0.32);
    float radius = 0.025 + grow * 0.135;

    float core = Clamp01(1.0 - dist / max(radius * 0.55, 0.0001));
    core = core * core * fade;

    float ringWidth = lerp(0.055, 0.13, progress);
    float ring = Clamp01(1.0 - abs(dist - radius) / ringWidth);
    ring = ring * ring * fade;

    float halo = Clamp01(1.0 - dist / (radius + 0.24));
    halo = halo * halo * fade;

    float sparkle = sin((dist - progress * 0.28) * 95.0 - progress * 20.0) * 0.5 + 0.5;
    float shock = ring * (0.72 + sparkle * 0.28);

    float distortion = (shock * 0.026 + halo * 0.008) * power * opacity;
    float2 uv = coords + radialUv * distortion;
    uv = float2(Clamp01(uv.x), Clamp01(uv.y));

    float3 baseColor = tex2D(uImage0, coords).rgb;
    float3 warped = tex2D(uImage0, uv).rgb;
    float mixAmount = Clamp01(shock * 0.78 + halo * 0.18);

    float3 color = lerp(baseColor, warped, mixAmount);
    color += uColor * core * 1.45 * power * opacity;
    color += lerp(uSecondaryColor, uColor, sparkle) * shock * 0.42 * power * opacity;
    color += uSecondaryColor * halo * 0.24 * power * opacity;

    return float4(color, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
