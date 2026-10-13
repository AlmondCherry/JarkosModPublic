sampler uImage0 : register(s0);
float uTime;
float uOpacity; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    
    float noise = sin(coords.x * 12.0 + uTime * 2.0) + cos(coords.y * 12.0 + uTime * 1.5);
    noise = noise * 0.25 + 0.5; 
    
    
    float2 dist = coords - float2(0.5, 0.5);
    float vignette = length(dist) * 1.5;
    
    
    float fogStrength = saturate(vignette) * uOpacity * 0.3;
    
    
    float4 fogColor = float4(0.2, 0.0, 0.3, 1.0); 
    
    
    color.rgb = lerp(color.rgb, fogColor.rgb, fogStrength * (0.5 + 0.5 * noise));
    
    return color;
}

technique Technique1
{
    pass Phase2FogPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
