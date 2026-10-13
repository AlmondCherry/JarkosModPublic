sampler uImage0 : register(s0);
float uTime;
float uOpacity; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    
    float noise = sin(coords.x * 20.0 + uTime * 2.0) * sin(coords.y * 20.0 + uTime * 2.5);
    
    
    float2 dist = coords - float2(0.5, 0.5);
    float vignette = length(dist) * 1.5;
    
    float fogStrength = smoothstep(0.0, 1.0, vignette) * uOpacity;
    
    float4 fogColor = float4(0.3, 0.0, 0.5, 1.0); 
    
    color.rgb = lerp(color.rgb, fogColor.rgb, fogStrength * (0.7 + 0.3 * noise));
    
    return color;
}

technique Technique1
{
    pass PurpleFogPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
