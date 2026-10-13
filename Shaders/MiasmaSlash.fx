







sampler uImage0 : register(s0);

float uProgress;
float uTime;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float alongRaw = abs(coords.x * 2.0 - 1.0);
    float across = 1.0 - abs(coords.y * 2.0 - 1.0);

    
    float gp = saturate(uProgress * 2.6);
    float grow = 1.0 - (1.0 - gp) * (1.0 - gp);
    
    float sh = saturate(uProgress * 2.0 - 1.0);

    float lenMask = saturate((grow - alongRaw) * 7.0);
    float taper = pow(saturate(1.0 - alongRaw), 0.35);
    float prof = saturate(across) * taper;

    
    float innerSoft = pow(prof, lerp(3.0, 60.0, sh));
    float inner = saturate((innerSoft - 0.25) * 2.2);
    
    float glow = pow(prof, lerp(1.2, 22.0, sh));

    float fadeOut = saturate(1.0 - (uProgress - 0.84) * 6.25);

    
    float2 uv1 = float2(coords.x * 4.0 + uTime * 0.050, coords.y * 0.5 + uTime * 0.020);
    float2 uv2 = float2(coords.x * 7.0 - uTime * 0.035, coords.y * 0.9 - uTime * 0.013);
    float4 t1 = tex2D(uImage0, uv1);
    float4 t2 = tex2D(uImage0, uv2);
    float stars = saturate(t1.r + t2.r * 0.75);
    float nebula = t1.g * 0.6 + t2.g * 0.4;

    float3 cosmos = float3(0.02, 0.01, 0.05)
        + float3(0.14, 0.07, 0.26) * nebula
        + float3(1.0, 0.97, 1.0) * stars;

    float innerA = inner * lenMask * fadeOut * uOpacity;
    float glowA = saturate(glow - inner) * lenMask * fadeOut * uOpacity * 0.85;

    
    float flashA = saturate(1.0 - uProgress * 3.5) * glow * lenMask * fadeOut * uOpacity * 0.4;

    
    float3 col = cosmos * innerA + float3(1.0, 1.0, 1.0) * (glowA + flashA);
    float a = saturate(innerA + glowA + flashA);

    return float4(col, a);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
