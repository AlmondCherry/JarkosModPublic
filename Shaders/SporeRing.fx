


sampler uImage0 : register(s0);

float uTime;
float uProgress;
float3 uColor;
float uSeed;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 rel = coords - float2(0.5, 0.5);
    float r = sqrt(rel.x * rel.x + rel.y * rel.y) * 2.0;

    float n = tex2D(uImage0, float2(coords.x * 1.2 + uSeed, coords.y * 1.2 + uSeed * 0.6 + uTime * 0.07)).r;

    float ringR = 0.08 + uProgress * 0.84;
    float wob = (n - 0.5) * 0.09 * (1.0 - uProgress * 0.4) + 0.010 * sin(uTime * 30.0 + (rel.x - rel.y) * 12.0);
    float rr = ringR + wob;
    float d = r - rr;
    float ad = abs(d);

    float w = 0.030 + 0.11 * (1.0 - uProgress * 0.55);
    float band = saturate((w - ad) * 14.0);
    float core = band * band;
    float fade = saturate(1.1 - uProgress * 1.1);

    
    float dOut = abs(d - w * 1.6);
    float dark = saturate((w - dOut) * 9.0) * 0.65 * fade;

    
    float inner = saturate((rr - r) * 2.2) * 0.16 * fade;

    
    float d2 = abs(r - rr * 0.72);
    float band2 = saturate((w * 0.8 - d2) * 12.0) * 0.5;

    
    float flash = saturate((0.40 - r) * 4.0) * saturate(1.0 - uProgress * 2.4);

    float grain = 0.7 + 0.6 * n;

    float3 bright = uColor * (band * 1.6 * grain + band2 + inner * 2.0)
                  + float3(1.0, 1.0, 0.85) * (core * 0.8 + flash);
    bright = bright * fade + uColor * flash * 0.8;

    float a = saturate(band * 1.25 + band2 * 0.6 + dark + inner + flash) * fade;
    float3 col = bright + float3(0.03, 0.10, 0.02) * dark;

    return float4(col, a);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
