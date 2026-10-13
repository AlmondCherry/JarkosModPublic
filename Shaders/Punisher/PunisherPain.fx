sampler uImage0 : register(s0);
float uOpacity; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float offset = 0.05 * uOpacity;
    float r = tex2D(uImage0, coords + float2(offset, 0)).r;
    float g = tex2D(uImage0, coords).g;
    float b = tex2D(uImage0, coords - float2(offset, 0)).b;
    
    float4 color = float4(r, g, b, 1.0);
    
    
    color.rgb = lerp(color.rgb, float3(0.8, 0.2, 1.0), uOpacity * 0.3);
    
    
    float2 dist = coords - float2(0.5, 0.5);
    float vignette = length(dist) * 2.0;
    color.rgb *= lerp(1.0, 1.0 - vignette, uOpacity);
    
    return color;
}

technique Technique1
{
    pass PainPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
