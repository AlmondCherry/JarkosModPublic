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

float SoftCircle(float2 p, float radius)
{
    float d = sqrt(dot(p, p) + 0.0001);
    float v = 1.0 - min(1.0, d / radius);
    return v * v;
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
    float intensity = Clamp01(uIntensity);
    float opacity = Clamp01(uOpacity);
    float pulse = 0.85 + 0.15 * sin(uTime * 8.0 + uProgress * 6.28318);

    float3 scene = tex2D(uImage0, coords).rgb;

    float core = SoftCircle(p, 0.055);
    float halo = SoftCircle(p, 0.190);
    float outerGlow = SoftCircle(p, 0.420);

    float horizontal = 1.0 - min(1.0, abs(p.y) / 0.006);
    horizontal *= 1.0 - min(1.0, abs(p.x) / 0.580);
    horizontal *= horizontal;

    float vertical = 1.0 - min(1.0, abs(p.x) / 0.004);
    vertical *= 1.0 - min(1.0, abs(p.y) / 0.180);
    vertical *= vertical * 0.45;

    float diagonalA = 1.0 - min(1.0, abs(p.x + p.y) / 0.010);
    diagonalA *= 1.0 - min(1.0, r / 0.310);
    diagonalA *= diagonalA * 0.35;

    float diagonalB = 1.0 - min(1.0, abs(p.x - p.y) / 0.010);
    diagonalB *= 1.0 - min(1.0, r / 0.310);
    diagonalB *= diagonalB * 0.35;

    float2 screenCenter = float2(0.5, 0.5);
    float2 flareAxis = screenCenter - center;

    float2 ghost1Pos = center + flareAxis * 0.45;
    float2 ghost2Pos = center + flareAxis * 0.82;
    float2 ghost3Pos = center + flareAxis * 1.22;

    float2 g1 = coords - ghost1Pos;
    float2 g2 = coords - ghost2Pos;
    float2 g3 = coords - ghost3Pos;
    g1.x *= aspect;
    g2.x *= aspect;
    g3.x *= aspect;

    float ghost1 = SoftCircle(g1, 0.055) * 0.40;
    float ghost2 = SoftCircle(g2, 0.080) * 0.24;
    float ghost3 = SoftCircle(g3, 0.110) * 0.16;

    float flare = 0.0;
    flare += core * 2.20;
    flare += halo * 0.80;
    flare += outerGlow * 0.25;
    flare += horizontal * 1.45;
    flare += vertical;
    flare += diagonalA + diagonalB;
    flare += ghost1 + ghost2 + ghost3;

    flare *= intensity * pulse;

    float3 warm = float3(1.0, 0.78, 0.36);
    float3 hot = float3(1.0, 0.96, 0.78);
    float3 blueEdge = float3(0.35, 0.65, 1.0);

    float3 flareColor = warm * flare;
    flareColor += hot * core * intensity * 1.5;
    flareColor += blueEdge * (ghost2 + ghost3) * intensity * 0.65;

    float distanceFromScreen = sqrt(dot(center - screenCenter, center - screenCenter));
    float edgeFade = 1.0 - min(1.0, distanceFromScreen * 1.15);
    flareColor *= edgeFade;

    float3 finalColor = scene + flareColor * opacity;
    return float4(finalColor, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}