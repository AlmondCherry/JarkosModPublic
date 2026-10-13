














sampler uImage0 : register(s0);










float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uIntensity;
float uProgress;
float uTime;

float Saturate01(float v)
{
    return min(1.0, max(0.0, v));
}

float SmoothStep01(float v)
{
    v = Saturate01(v);
    return v * v * (3.0 - 2.0 * v);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 uv = coords * 2.0 - 1.0;

    float r = length(uv);
    float a = atan2(uv.y, uv.x);

    
    
    float spriteAlpha = tex2D(uImage0, coords).a;

    
    
    float outerFalloff = 1.0 - SmoothStep01((r - 0.55) / 0.45);

    
    
    float coreHole = SmoothStep01((r - 0.04) / 0.10);

    
    
    float ring = 1.0 - Saturate01(abs(r - 0.78) / 0.05);
    ring = ring * ring * 0.45; 

    
    
    
    float spiralAngle = a * 4.0
        + log(max(r, 0.001)) * 6.0
        - uTime * 3.0;

    
    float bandA = sin(spiralAngle) * 0.5 + 0.5;
    float bandB = sin(spiralAngle * 0.5 + 1.7) * 0.5 + 0.5;
    float band = lerp(bandA, bandA * bandB, 0.6);
    band = pow(band, 1.7); 

    
    
    
    band *= lerp(1.0, 0.35, SmoothStep01((r - 0.45) / 0.45));
    band *= 0.55; 

    
    
    
    
    float radialBlend = SmoothStep01((0.85 - r) / 0.85);
    float3 baseBody = uColor * (0.18 + radialBlend * 0.55);

    
    float3 spiralColor = lerp(baseBody, uSecondaryColor, band * 0.55);

    
    float3 ringColor = lerp(uSecondaryColor, float3(1.0, 1.0, 1.0), 0.15);
    spiralColor += ringColor * ring * 0.45;

    
    
    
    float discAlpha = spriteAlpha * outerFalloff * coreHole;
    discAlpha = Saturate01(discAlpha);

    
    
    float collapseT = Saturate01((uProgress - 0.5) / 0.5);
    float collapseCut = 1.0 - SmoothStep01((r - (0.85 - collapseT * 0.7)) / 0.05);
    discAlpha *= collapseCut;

    
    
    discAlpha *= uSaturation;

    float3 finalColor = spiralColor * uIntensity;
    return float4(finalColor, discAlpha * uOpacity);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
