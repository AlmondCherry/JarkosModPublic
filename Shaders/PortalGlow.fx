









sampler uImage0 : register(s0);

float3 uColor;          
float3 uSecondaryColor; 
float uOpacity;         
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

    
    float spriteAlpha = tex2D(uImage0, coords).a;

    
    
    
    float pulse = 0.85 + 0.10 * sin(uTime * 4.0);
    float innerGlow = exp(-r * 5.0) * pulse * 0.55; 

    
    float midRing = 1.0 - Saturate01(abs(r - 0.6) / 0.18);
    midRing = midRing * midRing * 0.30; 

    
    float rim = 1.0 - Saturate01(abs(r - 0.78) / 0.04);
    rim = rim * rim;
    
    float angle = atan2(uv.y, uv.x);
    float shimmer = sin(angle * 9.0 - uTime * 6.0) * 0.5 + 0.5;
    rim *= (0.55 + 0.30 * shimmer) * 0.55; 

    
    
    
    float collapseT = Saturate01((uProgress - 0.5) / 0.5);
    float collapseFlash = exp(-r * (3.0 - collapseT * 2.5)) * collapseT * 0.55;

    
    float3 col = uColor * innerGlow;
    col += uColor * midRing * 0.45;
    col += uSecondaryColor * rim;
    col += float3(1.0, 1.0, 1.0) * collapseFlash;

    
    float alpha = (innerGlow * 0.55 + midRing * 0.45 + rim + collapseFlash);
    alpha = Saturate01(alpha) * spriteAlpha * uOpacity * uIntensity;

    return float4(col, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
