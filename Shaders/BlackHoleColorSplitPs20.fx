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
    float progress = uProgress;
    float intensity = uIntensity;
    float opacity = uOpacity;

    float fade = 1.0 - progress;
    fade = fade * fade;
    float impact = 1.0 - progress * 2.0;
    impact = max(0.0, min(1.0, impact));
    float amount = fade * intensity * opacity * (0.006 + impact * 0.010);

    float2 redUv = coords + float2(amount, 0.0);
    float2 blueUv = coords - float2(amount, 0.0);

    float red = tex2D(uImage0, redUv).r;
    float green = tex2D(uImage0, coords).g;
    float blue = tex2D(uImage0, blueUv).b;

    float3 color = float3(red, green, blue);
    color.r = color.r + uColor.r * impact * 0.05;
    color.g = color.g + uColor.g * impact * 0.05;
    color.b = color.b + uColor.b * impact * 0.05;

    float keepAlive = uTime * 0.0 + uScreenResolution.x * 0.0 + uScreenPosition.x * 0.0 + uTargetPosition.x * 0.0 + uImageOffset.x * 0.0 + uDirection.x * 0.0 + uZoom.x * 0.0 + uSecondaryColor.x * 0.0;
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
