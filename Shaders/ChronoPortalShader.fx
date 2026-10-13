
sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uScale;
float uTime;

float Saturate01(float v)
{
    return min(1.0, max(0.0, v));
}

float SmoothStep01(float v)
{
    v = min(1.0, max(0.0, v));
    return v * v * (3.0 - 2.0 * v);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float pixelGrid = 52.0;
    float2 pixelatedCoords = floor(coords * pixelGrid) / pixelGrid;
    
    
    float2 uv = pixelatedCoords * 2.0 - 1.0;
    
    float r = length(uv);
    float a = atan2(uv.y, uv.x);
    
    
    float spriteAlpha = tex2D(uImage0, pixelatedCoords).a;
    
    if (r > 0.95)
        return float4(0.0, 0.0, 0.0, 0.0);
        
    
    float spiralAngle1 = a * 4.0 + log(max(r, 0.002)) * 6.5 - uTime * 9.0;
    float spiralAngle2 = a * 4.0 + log(max(r, 0.002)) * 6.5 + 3.14159 - uTime * 9.0;
    
    float spiral1 = sin(spiralAngle1) * 0.5 + 0.5;
    float spiral2 = sin(spiralAngle2) * 0.5 + 0.5;
    float spiral = max(pow(spiral1, 2.5), pow(spiral2, 2.5));
    
    
    float ripple = sin(r * 22.0 - uTime * 14.0) * 0.5 + 0.5;
    ripple = pow(ripple, 3.0) * 0.25 * (1.0 - r);
    
    
    float accretion = 1.0 - Saturate01(abs(r - 0.22) / 0.12);
    accretion = pow(accretion, 2.5) * 0.4;
    
    
    float coreHole = SmoothStep01((r - 0.08) / 0.16);
    
    
    float radial = 1.0 - r;
    radial = max(0.0, radial);
    
    
    float3 col = lerp(uColor * 0.35, uSecondaryColor, (spiral + ripple + accretion) * radial);
    col *= coreHole; 
    
    float3 finalCol = col * 1.8;
    
    
    float alpha = spriteAlpha * uOpacity * (1.0 - SmoothStep01((r - 0.7) / 0.25));
    
    return float4(finalCol * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
