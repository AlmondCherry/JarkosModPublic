sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords;
    float2 center = (uTargetPosition - uScreenPosition) / uScreenResolution;

    float2 p = uv - center;
    p.x *= uScreenResolution.x / uScreenResolution.y;

    float2 dir = uDirection;
    float dirLen = length(dir);
    if (dirLen < 0.001)
        dir = float2(1.0, 0.0);
    else
        dir /= dirLen;

    float2 tangent = float2(-dir.y, dir.x);

    float radius = length(p);
    float along = dot(p, dir);
    float side = dot(p, tangent);

    float mainMask = 1.0 - saturate(radius / max(uSecondaryColor.r, 0.001));
    mainMask = mainMask * mainMask * (3.0 - 2.0 * mainMask);

    float tailMask = 1.0 - saturate(abs(side) / max(uSecondaryColor.g, 0.001));
    float behind = 1.0 - saturate((along + uSecondaryColor.b * 0.08) / max(uSecondaryColor.b * 0.08, 0.001));
    float tailEnd = 1.0 - saturate((-along - uSecondaryColor.b) / max(uSecondaryColor.b * 0.35, 0.001));
    float frontFade = 1.0 - saturate(along / max(uSecondaryColor.b * 0.25, 0.001));
    tailMask *= behind * tailEnd * frontFade;
    tailMask = tailMask * tailMask * (3.0 - 2.0 * tailMask);

    float ribs = sin(along * 165.0 - uTime * 12.0) * 0.5 + 0.5;
    ribs *= sin(side * 95.0 + uTime * 8.0) * 0.25 + 0.75;

    float edgeSplit = 1.0 - saturate(distance(p, dir * uColor.b) / max(uSecondaryColor.g * 1.25, 0.001));
    edgeSplit = edgeSplit * edgeSplit * (3.0 - 2.0 * edgeSplit);

    float trail = max(mainMask * 0.65, tailMask);
    float strength = uIntensity * uOpacity * trail;
    float ribStrength = strength * (0.65 + ribs * 0.85);

    float2 offset = dir * (0.017 * ribStrength + 0.008 * strength);
    offset += tangent * sin(side * 120.0 + along * 45.0 - uTime * 10.0) * 0.011 * ribStrength;
    offset += tangent * ribs * tailMask * uOpacity * uIntensity * 0.009;

    float2 distortedUv = uv + offset;

    float chroma = (0.0022 * strength + 0.0045 * edgeSplit) * uColor.g;
    float red = tex2D(uImage0, distortedUv + dir * chroma + tangent * chroma * 0.45).r;
    float green = tex2D(uImage0, distortedUv).g;
    float blue = tex2D(uImage0, distortedUv - dir * chroma - tangent * chroma * 0.45).b;

    float rim = edgeSplit * uOpacity * uIntensity;
    float3 color = float3(red, green, blue);
    color += float3(0.04, 0.01, 0.08) * tailMask * ribs * uOpacity;
    color += float3(0.05, 0.02, 0.10) * rim;

    return float4(color, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
