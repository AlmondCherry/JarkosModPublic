float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float Hash21(float2 value)
{
    value = frac(value * float2(127.1, 311.7));
    value += dot(value, value + 37.13);
    return frac(value.x * value.y);
}

float Noise(float2 value)
{
    float2 cell = floor(value);
    float2 local = frac(value);
    local = local * local * (3.0 - 2.0 * local);

    float a = Hash21(cell);
    float b = Hash21(cell + float2(1.0, 0.0));
    float c = Hash21(cell + float2(0.0, 1.0));
    float d = Hash21(cell + float2(1.0, 1.0));

    return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float opacity = Clamp01(uOpacity) * vertexColor.a;

    float2 centered = coords - float2(0.5, 0.55);
    float distanceToCenter = length(centered * float2(0.85, 1.15));
    float body = Clamp01(1.0 - distanceToCenter * 1.05);
    float edge = pow(Clamp01(1.0 - abs(coords.y - 0.5) * 1.2), 0.9);
    float pulse = 0.88 + 0.12 * sin(uSaturation * 5.0 + coords.y * 8.0);

    float noise = Noise(coords * float2(11.0, 16.0) + float2(uSaturation * 1.6, -uSaturation * 2.0));
    float cracks = pow(Clamp01(noise * 1.7 - 0.34), 1.8);
    float stars = step(0.965, Hash21(floor(coords * 34.0 + uSaturation * 0.7)));
    float alpha = Clamp01((body * 0.36 + cracks * 0.7 + stars * 0.75) * edge * pulse * opacity);

    float3 color = float3(0.0, 0.0, 0.0);
    color += uSecondaryColor * (cracks * 0.55 + stars * 0.9);
    color += float3(0.1, 0.02, 0.18) * body * 0.22;

    return float4(color, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
