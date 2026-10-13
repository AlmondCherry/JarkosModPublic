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
    float opacity = Clamp01(uOpacity);
    float intensity = Clamp01(uIntensity);

    float aspect = uScreenResolution.x / max(uScreenResolution.y, 1.0);

    float pixelCount = 220.0 + (55.0 - 220.0) * intensity;
    float2 pixelGrid = float2(pixelCount * aspect, pixelCount);

    float2 pixelUv = floor(coords * pixelGrid) / pixelGrid;
    pixelUv.x = Clamp01(pixelUv.x);
    pixelUv.y = Clamp01(pixelUv.y);

    float3 original = tex2D(uImage0, coords).rgb;
    float3 pixelated = tex2D(uImage0, pixelUv).rgb;

    float gridX = frac(coords.x * pixelGrid.x);
    float gridY = frac(coords.y * pixelGrid.y);

    float verticalGrid = 1.0 - min(1.0, gridX / 0.045);
    float horizontalGrid = 1.0 - min(1.0, gridY / 0.045);
    float gridMask = min(1.0, verticalGrid + horizontalGrid);

    pixelated *= 1.0 - gridMask * 0.18 * intensity;

    float scan = sin(coords.y * uScreenResolution.y * 3.14159);
    scan = 0.92 + scan * 0.035 * intensity;
    pixelated *= scan;

    float2 center = coords - float2(0.5, 0.5);
    float vignette = 1.0 - dot(center, center) * 0.65 * intensity;
    pixelated *= Clamp01(vignette);

    float keepAlive = 0.0;
    keepAlive += uColor.x * 0.000001 + uSecondaryColor.x * 0.000001;
    keepAlive += uTime * 0.000001;
    keepAlive += uScreenPosition.x * 0.000001 + uTargetPosition.x * 0.000001;
    keepAlive += uImageOffset.x * 0.000001 + uDirection.x * 0.000001;
    keepAlive += uProgress * 0.000001 + uZoom.x * 0.000001;
    pixelated += float3(keepAlive, keepAlive, keepAlive);

    float3 finalColor = lerp(original, pixelated, opacity);
    return float4(finalColor, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}