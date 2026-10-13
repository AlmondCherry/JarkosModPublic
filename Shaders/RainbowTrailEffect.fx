sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uImageOffset;

float Clamp01(float value)
{
    return max(0.0, min(1.0, value));
}

float3 Rainbow(float phase)
{
    float r = sin(phase * 6.28318) * 0.5 + 0.5;
    float g = sin(phase * 6.28318 + 2.09439) * 0.5 + 0.5;
    float b = sin(phase * 6.28318 + 4.18878) * 0.5 + 0.5;
    return float3(r, g, b);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 tex = tex2D(uImage0, coords);

    float x = coords.x; 
    float y = abs(coords.y - 0.5) * 2.0;

    float center = 1.0 - Clamp01(y);
    center = center * center;

    float headFade = Clamp01(x * 1.15);
    float tailFade = Clamp01((1.0 - x) * 1.35);
    float lengthFade = headFade * tailFade;

    float softBody = center * lengthFade;

    float heat = Clamp01(x * 1.35);
    float flicker = sin(uTime * 8.0 + x * 5.0 + uProgress * 4.0) * 0.5 + 0.5;
    flicker = 0.88 + flicker * 0.12;

    float phase = uTime * 0.16 + x * 0.35 + uProgress;
    float3 rainbow = Rainbow(phase);

    float3 orange = float3(1.0, 0.28, 0.035);
    float3 yellow = float3(1.0, 0.78, 0.18);
    float3 whiteHot = float3(1.0, 0.95, 0.72);

    float3 color = lerp(rainbow, orange, 0.18);
    color = lerp(color, yellow, center * heat * 0.55);
    color = lerp(color, whiteHot, center * center * heat * 0.35);

    float outer = 1.0 - Clamp01(y * 0.72);
    outer = outer * outer * lengthFade;
    color += rainbow * outer * 0.24;

    float alpha = tex.a;
    alpha *= softBody;
    alpha *= flicker;
    alpha *= uOpacity;

    color *= uIntensity;
    color *= 0.75 + heat * 0.65;

    float keepAlive = 0.0;
    keepAlive += uColor.x * 0.000001 + uSecondaryColor.x * 0.000001;
    keepAlive += uDirection.x * 0.000001 + uImageOffset.x * 0.000001;
    color += float3(keepAlive, keepAlive, keepAlive);

    return float4(color, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}