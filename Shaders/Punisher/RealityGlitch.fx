sampler uImage0 : register(s0);

float uTime;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float glitchTime = uTime * 5.0;
    
    
    
    float shift = sin(coords.y * 50.0 + glitchTime) * 0.05 * uOpacity;
    
    
    float4 colBase = tex2D(uImage0, coords);
    
    
    if (colBase.a == 0.0) 
        return float4(0, 0, 0, 0);
    
    
    float2 redCoords = coords + float2(shift, 0);
    float2 blueCoords = coords - float2(shift, 0);
    
    float4 redCol = tex2D(uImage0, redCoords);
    float4 blueCol = tex2D(uImage0, blueCoords);
    
    
    float4 finalCol = float4(redCol.r, colBase.g, blueCol.b, colBase.a);
    
    
    float3 neonTint = float3(0.5, 0.0, 0.5) * uOpacity * finalCol.a;
    finalCol.rgb += neonTint;
    
    
    finalCol = saturate(finalCol);
    
    
    return finalCol * color;
}

technique Technique1
{
    pass RealityGlitch
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
