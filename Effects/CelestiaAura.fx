sampler uImage0 : register(s0);

float uTime;
float uPhase;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    float waveX = coords.x * 20.0;
    float waveY = coords.y * 30.0;
    float timeVal = uTime * 5.0;
    
    float wave = sin(waveX + waveY - timeVal);
    float wave2 = cos(-coords.x * 15.0 + coords.y * 25.0 + uTime * 4.0);
    float n = (wave * wave2) * 0.5 + 0.5;
    
    float r = 0.1 + 0.3 * n;
    float g = 0.5 + 0.4 * n;
    float b = 1.0;
    
    float intensity = clamp((uPhase - 1.0) * 0.8, 0.0, 2.0) + 0.2;
    
    float alpha = color.a * n * intensity;
    
    return float4(r * alpha, g * alpha, b * alpha, alpha);
}

technique Technique1
{
    pass CelestiaAuraPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
