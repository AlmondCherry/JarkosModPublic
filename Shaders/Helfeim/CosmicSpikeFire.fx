sampler uImage0 : register(s0);

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
    float time = uSaturation;
    float y = 1.0 - coords.y;

    float sway = sin(y * 10.0 - time * 4.8) * 0.052 + sin(y * 22.0 - time * 7.2) * 0.032;
    float2 noiseUv = coords * float2(6.2, 7.4) + float2(sway, time * 1.95);
    float n1 = Noise(noiseUv);
    float n2 = Noise(coords * float2(15.5, 5.2) + float2(sway * 2.0, time * 2.7 + 6.7));
    float ribsA = 0.5 + 0.5 * sin(coords.x * 54.0 + y * 10.0 + time * 2.4);
    float ribsB = 0.5 + 0.5 * sin(coords.x * 82.0 - y * 17.0 - time * 3.1);
    float ribs = ribsA * 0.58 + ribsB * 0.42;
    float noise = n1 * 0.48 + n2 * 0.28 + ribs * 0.24;

    float width = lerp(0.4, 0.04, y);
    width += noise * 0.055 * (1.0 - y);
    float center = 0.5 + (n1 - 0.5) * 0.09 + sway;
    float distanceToCenter = abs(coords.x - center);
    float body = Clamp01(1.0 - distanceToCenter / width);
    body = body * body * (3.0 - 2.0 * body);

    float ribMask = Clamp01((ribs - 0.32) * 1.45);
    float tongues = Clamp01(noise * 0.92 + ribMask * 0.42 + body * 0.62 - y * 0.6);
    float flame = body * tongues * tongues;
    flame *= smoothstep(1.0, 0.72, y) * smoothstep(0.0, 0.025, y);

    float coreWidth = width * 0.34;
    float core = Clamp01(1.0 - distanceToCenter / coreWidth);
    core = core * core * tongues;

    float smokeWidth = width * 1.55;
    float smoke = Clamp01(1.0 - distanceToCenter / smokeWidth) * (1.0 - body * 0.65) * Clamp01(1.0 - y * 0.55);
    smoke *= 0.55 + noise * 0.35;

    float sparkNoise = Noise(coords * float2(11.0, 15.0) + float2(time * 0.7, time * 2.4));
    float embers = Clamp01((sparkNoise - 0.78) * 4.5) * Clamp01(flame + smoke);

    float3 smokeColor = float3(0.01, 0.003, 0.018);
    float3 deepPurple = float3(0.08, 0.0, 0.16);
    float3 purple = float3(0.34, 0.015, 0.68);
    float3 hotViolet = float3(0.82, 0.22, 0.92);

    float3 color = smokeColor * smoke;
    color += deepPurple * flame * 0.88;
    color += purple * flame * (0.3 + ribMask * 0.45);
    color += hotViolet * core * (0.2 + ribMask * 0.12);
    color += uSecondaryColor * embers * 0.36;

    float alpha = Clamp01((flame * 0.92 + core * 0.32 + smoke * 0.32 + embers * 0.25) * opacity);
    return float4(color, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
