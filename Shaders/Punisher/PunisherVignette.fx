sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uTargetPosition;
float4 uShaderSpecificData;


float Intensity; 
float Pulse;     

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center);
    
    
    
    float totalIntensity = 0.4 + (Intensity * 0.6);
    
    
    
    float radius = 0.9 - (totalIntensity * 0.35) - (Pulse * 0.08);
    float softness = 0.5; 
    
    float vignette = smoothstep(radius, radius - softness, dist);
    
    
    float darken = 1.0 - vignette;
    darken *= totalIntensity; 
    
    
    float3 vignetteColor = float3(0.25, 0.0, 0.0);
    
    
    float pulseEffect = Pulse * 0.4;
    
    
    float3 finalColor = lerp(color.rgb, vignetteColor, darken * (0.6 + pulseEffect));
    
    return float4(finalColor, color.a);
}

technique Technique1
{
    pass Vignette
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
