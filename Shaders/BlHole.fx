sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float2 uTargetPosition;
float uTime;
float2 uScreenResolution;
float2 uScreenPosition;
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

    float2 center;
    center.x = (uTargetPosition.x - uScreenPosition.x) / res.x;
    center.y = (uTargetPosition.y - uScreenPosition.y) / res.y;
    center.x = Clamp01(center.x);
    center.y = Clamp01(center.y);

    float aspect = res.x / res.y;

    float2 p = coords - center;
    p.x *= aspect;

    float r = sqrt(dot(p, p) + 0.0001);
    float2 radial = p / (r + 0.0001);
    float2 tangent = float2(-radial.y, radial.x);

    float intensity = Clamp01(uIntensity);
    float opacity = Clamp01(uOpacity);
    float pulse = Clamp01(uProgress);
    float zoom = max(uZoom.x, 0.30);

    float lensRadius = 0.30;
    float lensMask = 1.0 - min(1.0, r / lensRadius);
    lensMask = lensMask * lensMask;

    float horizonRadius = 0.060 + pulse * 0.006;
    float horizon = 1.0 - min(1.0, r / horizonRadius);
    horizon = horizon * horizon;

    float gravity = lensMask * intensity * 0.045 / (r * 4.0 + 0.28);
    float spin = lensMask * intensity * 0.035 / zoom;

    float2 uv = coords + radial * gravity + tangent * spin;
    uv.x = Clamp01(uv.x);
    uv.y = Clamp01(uv.y);

    float chroma = lensMask * intensity * 0.0035;
    float2 uvR = uv + float2(chroma, 0.0);
    float2 uvB = uv - float2(chroma, 0.0);
    uvR.x = Clamp01(uvR.x);
    uvB.x = Clamp01(uvB.x);

    float3 original = tex2D(uImage0, coords).rgb;

    float red = tex2D(uImage0, uvR).r;
    float green = tex2D(uImage0, uv).g;
    float blue = tex2D(uImage0, uvB).b;
    float3 col = float3(red, green, blue);

    float diskY = abs(p.y + sin(p.x * 18.0 + uTime * 3.0) * 0.012);
    float diskLength = 1.0 - min(1.0, abs(p.x) / 0.24);
    float diskThin = 1.0 - min(1.0, diskY / 0.030);
    float diskHole = min(1.0, r / 0.075);
    float disk = diskLength * diskThin * diskHole;
    disk = disk * disk;

    float hotCore = 1.0 - min(1.0, abs(diskY) / 0.018);
    hotCore = hotCore * diskLength * diskHole;

    float3 diskColor = float3(1.0, 0.48, 0.10) * disk;
    diskColor += float3(1.0, 0.88, 0.55) * hotCore * 0.55;

    float rim = 1.0 - min(1.0, abs(r - horizonRadius) * 42.0);
    rim = rim * rim;

    col += diskColor * intensity;
    col += float3(0.75, 0.90, 1.0) * rim * lensMask * 0.18 * intensity;

    col = lerp(col, float3(0.0, 0.0, 0.0), Clamp01(horizon * 1.35));

    float darkLens = lensMask * 0.18 * intensity;
    col *= 1.0 - darkLens;

    float keepAlive = 0.0;
    keepAlive += uColor.x * 0.000001 + uSecondaryColor.x * 0.000001;
    keepAlive += uImageOffset.x * 0.000001 + uDirection.x * 0.000001;

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