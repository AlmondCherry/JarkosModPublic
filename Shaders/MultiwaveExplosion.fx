sampler uImage0 : register(s0); 

float uTime;
float uProgress; 
float3 uColor;   
float uSeed;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 rel = coords - float2(0.5, 0.5);
    float r = length(rel) * 2.0; 

    
    float n = tex2D(uImage0, float2(coords.x * 1.3 + uSeed, coords.y * 1.3 - uTime * 0.15)).r;

    
    float ringR = 0.05 + uProgress * 0.92;
    
    
    float wobble = (n - 0.5) * 0.12 * (1.0 - uProgress * 0.5);
    float rr = ringR + wobble;
    
    float d = r - rr;
    float ad = abs(d);

    
    float w = 0.040 + 0.12 * (1.0 - uProgress * 0.6);
    float band = saturate((w - ad) * 12.0);
    float core = band * band;
    
    float fade = saturate(1.0 - uProgress);

    
    float3 brightRing = uColor * (band * 1.5 + core * 0.8) * (0.6 + 0.4 * n);
    
    
    float flash = saturate((0.35 - r) * 5.0) * saturate(1.0 - uProgress * 3.0);
    
    float3 col = brightRing * fade + float3(1.0, 0.95, 0.8) * flash * 0.9;
    float alpha = saturate(band * 1.3 + flash) * fade;

    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
