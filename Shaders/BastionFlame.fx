





sampler uImage0 : register(s0);

float uTime;
float uIntensity;
float uSeed;
float uBurst;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float t = uTime;

    float n1 = tex2D(uImage0, float2(coords.x * 0.9 + uSeed, coords.y * 0.7 + t * 0.5 + uSeed)).r;
    float n2 = tex2D(uImage0, float2(coords.x * 1.7 - uSeed, coords.y * 1.2 + t * 0.9)).g;
    float n = n1 * 0.6 + n2 * 0.5;

    
    
    float relx = (coords.x - 0.5) * 0.73333; 
    float rely = coords.y - 0.55;
    float by = rely - min(max(rely, -0.07), 0.08);
    float r = sqrt(relx * relx + by * by);

    
    float up = saturate(-rely * 1.6);
    float rr = r - (n - 0.5) * (0.07 + up * (0.30 + uBurst * 0.55)) * (1.0 + uBurst * 0.5);

    float shell = 0.13 * (1.0 + uBurst * 0.5);
    float f = saturate((shell - rr) * 6.0) * uIntensity;

    
    f = f * saturate((0.105 - rely) * 22.0);

    
    f = f * (0.88 + 0.12 * sin(t * 9.0 + coords.y * 13.0 + uSeed * 40.0));

    float f2 = f * f;
    float3 col = float3(f * 2.1, f2 * 1.65, f2 * f * 1.2) * (1.0 + uBurst * 1.2);
    float a = saturate(f * 1.4) * 0.85;

    return float4(col, a);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
