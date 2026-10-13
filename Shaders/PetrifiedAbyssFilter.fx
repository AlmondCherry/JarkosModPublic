






sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uTime;
float uIntensity;
float uProgress;
float uDirection;
float uRotation;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageSize0;
float2 uImageSize1;
float3 uLightSource;
float4 uSourceRect;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, coords);

    
    float lum = dot(base.rgb, float3(0.299, 0.587, 0.114));

    
    
    float3 highlight = uColor * saturate(lum * 1.6);
    float3 midtone = uSecondaryColor * (1.0 - abs(lum - 0.5) * 1.6);
    float3 shadow = uSecondaryColor * 0.25 * (1.0 - lum);

    float3 graded = highlight + midtone * 0.35 + shadow;

    
    
    float3 desat = lerp(base.rgb, graded, 0.85);
    float3 result = lerp(base.rgb, desat, uOpacity);

    return float4(result, base.a);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
