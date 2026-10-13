sampler uImage0 : register(s0);

float2 uScreenResolution;
float2 uSkyOffset;
float uTime;
float uOpacity;
float uIntensity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float aspect = uScreenResolution.x / (uScreenResolution.y + 1.0);
    float time = uTime * 0.35;

    float x = (coords.x - 0.5) * aspect;
    float y = coords.y - 0.5;
    y = y - 0.12;

    float radius2 = x * x + y * y + 0.035;
    float tunnel = 1.0 / radius2;

    float driftX = uSkyOffset.x + sin(time * 0.37) * 0.06;
    float driftY = uSkyOffset.y + cos(time * 0.29) * 0.05;

    float uvx = x * tunnel * 0.055 + driftX + time * 0.12;
    float uvy = y * tunnel * 0.055 + driftY - time * 0.08;

    float cellA = abs(sin(uvx * 18.0 + sin(uvy * 2.0 + time)));
    cellA = cellA + abs(sin(uvy * 22.0 - time * 1.31));

    float cellB = abs(sin((uvx + 0.07) * 28.0 + time * 0.73));
    cellB = cellB + abs(sin((uvy - 0.04) * 16.0 - time * 1.17));

    float cellC = abs(sin((uvx - 0.05) * 36.0 + time * 1.43));
    cellC = cellC + abs(sin((uvy + 0.03) * 31.0 - time * 0.91));

    float dustA = 0.055 / (cellA + 0.045);
    float dustB = 0.040 / (cellB + 0.040);
    float dustC = 0.032 / (cellC + 0.035);

    float beamCenter = abs(y + sin(time + x * 1.7) * 0.11);
    float beam = 0.020 / (beamCenter + 0.025);
    beam = beam * (1.0 - coords.y * 0.60);

    float core = 1.0 - radius2 * 1.8;
    core = saturate(core);

    float veil = sin(radius2 * 45.0 - time * 5.0);
    veil = veil * 0.5 + 0.5;
    veil = veil * core;

    float fadeTop = coords.y - 0.04;
    fadeTop = saturate(fadeTop * 9.0);

    float fadeBottom = 0.96 - coords.y;
    fadeBottom = saturate(fadeBottom * 9.0);

    float vignette = 1.10 - radius2 * 0.65;
    vignette = saturate(vignette);

    float power = 0.75 + uIntensity * 0.70;
    float volume = dustA * 0.70 + dustB * 0.45 + dustC * 0.35 + beam * 0.65 + veil * 0.22;
    volume = volume * power * fadeTop * fadeBottom * vignette;

    float red = 0.020 + core * 0.060 + volume * 0.32;
    float green = 0.035 + core * 0.140 + volume * 0.78;
    float blue = 0.060 + core * 0.260 + volume * 1.30;

    float cyan = beam * fadeTop * fadeBottom * 0.22;
    green = green + cyan * 0.80;
    blue = blue + cyan;

    return float4(red, green, blue, uOpacity);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
