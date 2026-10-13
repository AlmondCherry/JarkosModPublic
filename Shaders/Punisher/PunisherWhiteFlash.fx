sampler uImage0 : register(s0);
float uOpacity; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    
    float3 inverted = 1.0 - color.rgb;
    
    
    float lum = dot(inverted, float3(0.299, 0.587, 0.114));
    
    
    
    float manga = (lum - 0.4) * 20.0 + 0.5;
    
    if (uOpacity > 0.9)
    {
        
        return float4(1.0, 1.0, 1.0, color.a);
    }
    else if (uOpacity > 0.0)
    {
        
        float blend = uOpacity * 1.5; 
        if (blend > 1.0) blend = 1.0; 
        
        return lerp(color, float4(manga, manga, manga, color.a), blend);
    }
    
    return color; 
}

technique Technique1
{
    pass PunisherWhiteFlash
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
