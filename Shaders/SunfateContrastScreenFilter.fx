






sampler uImage0 : register(s0);

float uIntensity; 

float4 PixelFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float3 rgb = color.rgb;

    
    float contrastFactor = 1.0 + 1.25 * saturate(uIntensity);
    float3 contrasted = (rgb - 0.5) * contrastFactor + 0.5;

    
    float darkenFactor = 1.0 - 0.38 * saturate(uIntensity);
    float3 darkened = contrasted * darkenFactor;

    
    float3 finalRgb = lerp(rgb, saturate(darkened), saturate(uIntensity));

    return float4(finalRgb, color.a);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelFunction();
    }
}
