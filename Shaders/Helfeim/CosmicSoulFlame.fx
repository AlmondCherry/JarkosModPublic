sampler uImage0 : register(s0);

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
    value = frac(value * float2(131.27, 297.71));
    value += dot(value, value + 41.63);
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
    float time = uSaturation;
    float2 p = coords * 2.0 - 1.0;
    p.y = -p.y;

    float height = coords.y;
    float tip = 1.0 - height;
    float sway = sin(time * 5.2 + height * 9.0) * 0.055 + sin(time * 8.1 - height * 17.0) * 0.025;
    float x = p.x + sway * tip;
    float y = p.y;

    float n1 = Noise(coords * float2(5.5, 8.5) + float2(time * 0.38, -time * 1.65));
    float n2 = Noise(coords * float2(14.0, 19.0) + float2(-time * 0.55, time * 2.25));
    float noise = n1 * 0.62 + n2 * 0.38;

    float bodyWidth = 0.34 * (1.0 - height) + 0.08;
    bodyWidth = bodyWidth + noise * 0.035 * tip;
    float body = 1.0 - sqrt(x * x + y * y * 0.18) / bodyWidth;
    body = Clamp01(body);
    body = body * body * (3.0 - 2.0 * body);

    float coreWidth = bodyWidth * 0.5;
    float core = 1.0 - sqrt(x * x + y * y * 0.34) / coreWidth;
    core = Clamp01(core);
    core = core * core;

    float smokeWidth = bodyWidth * 1.45;
    float smoke = 1.0 - sqrt(x * x + y * y * 0.11) / smokeWidth;
    smoke = Clamp01(smoke) * Clamp01(1.0 - core * 0.55);
    smoke = smoke * (0.45 + noise * 0.55);

    float tipSpark = 1.0 - sqrt(x * x * 0.7 + (y - 0.58) * (y - 0.58)) / 0.3;
    tipSpark = Clamp01(tipSpark);
    tipSpark = tipSpark * tipSpark * (0.55 + noise * 0.45);

    float3 softWhite = float3(0.88, 0.92, 1.0);
    float3 hotWhite = float3(1.0, 0.96, 1.0);
    float3 paleBlue = float3(0.55, 0.74, 1.0);
    float3 violet = float3(0.55, 0.08, 1.0);
    float3 deepViolet = float3(0.12, 0.0, 0.24);

    float3 color = deepViolet * smoke * 0.18;
    color = color + softWhite * body * 0.72;
    color = color + paleBlue * body * 0.12;
    color = color + hotWhite * core * 0.58;
    color = color + violet * tipSpark * 0.72;
    color = color + uSecondaryColor * smoke * 0.16;

    float alpha = body * 0.76 + core * 0.42 + smoke * 0.12 + tipSpark * 0.28;
    alpha = Clamp01(alpha * opacity);

    return float4(clamp(color, 0.0, 1.0), alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
