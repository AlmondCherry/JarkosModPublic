sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float2 uScreenResolution;
float uIntensity;
float uOpacity;
float2 uTargetPosition; 
float uRadius;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    
    float2 pixelPos = coords * uScreenResolution;
    
    
    float dist = distance(pixelPos, uTargetPosition);
    
    
    
    
    
    
    
    
    
    float darkness = saturate((uRadius - dist) / 100.0);
    
    color.rgb = lerp(color.rgb, float3(0,0,0), darkness * uOpacity);
    
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
