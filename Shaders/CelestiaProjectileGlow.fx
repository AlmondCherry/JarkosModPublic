







sampler uImage0 : register(s0);

float  uTime;
float  uOpacity;
float  uPulse;
float3 uCoreColor;
float3 uGlowColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float dx = coords.x - 0.5;
    float dy = coords.y - 0.5;
    float dist = sqrt(dx * dx + dy * dy) * 2.0;

    
    float p = sin(uTime * 4.5) * 0.5 + 0.5;
    float pulse = 0.7 + p * 0.3 * (0.5 + uPulse);

    
    float core = saturate(1.0 - dist * 1.4);
    core = core * core;

    float halo = saturate(1.0 - dist);
    halo = halo * halo * halo;

    
    float hue1 = sin(coords.x * 4.0 + uTime * 1.0) * 0.5 + 0.5;
    float hue2 = sin(coords.y * 3.0 - uTime * 0.8 + 1.5) * 0.5 + 0.5;
    float h = hue1 * 0.6 + hue2 * 0.4;

    float3 baseCol;
    baseCol.x = uGlowColor.x * (0.7 + h * 0.3);
    baseCol.y = uGlowColor.y * (0.7 + h * 0.3);
    baseCol.z = uGlowColor.z * (0.7 + h * 0.3);

    float3 col;
    col.x = baseCol.x * halo + uCoreColor.x * core;
    col.y = baseCol.y * halo + uCoreColor.y * core;
    col.z = baseCol.z * halo + uCoreColor.z * core;

    col.x = col.x * pulse;
    col.y = col.y * pulse;
    col.z = col.z * pulse;

    float alpha = saturate(halo * 0.55 + core * 0.85) * uOpacity;

    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
