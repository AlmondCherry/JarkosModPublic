sampler uImage0 : register(s0);
float uOpacity; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    if (uOpacity <= 0.0) return tex2D(uImage0, coords);

    
    float cols = 12.0;
    float rows = 8.0;
    
    float xPos = coords.x * cols;
    float yPos = coords.y * rows;
    
    
    
    float distToVerticalLine = abs(frac(xPos + 0.5) - 0.5) / cols * 1.77; 
    float distToHorizontalLine = abs(frac(yPos + 0.5) - 0.5) / rows;
    
    float distToGrid = min(distToVerticalLine, distToHorizontalLine);
    
    
    float jaggedness = sin(coords.x * 200.0 + coords.y * 200.0) * 0.001 * uOpacity;
    distToGrid += jaggedness;
    
    float2 finalCoords = coords;
    
    
    if (distToGrid < 0.004 * uOpacity)
    {
        float2 pushNormal = float2(0.0, 0.0);
        if (distToVerticalLine < distToHorizontalLine)
        {
            pushNormal = float2(sign(frac(xPos) - 0.5), 0.0);
        }
        else
        {
            pushNormal = float2(0.0, sign(frac(yPos) - 0.5));
        }
        
        finalCoords += pushNormal * 0.015 * uOpacity;
    }
    
    
    float2 dir = float2(0.005, 0.005);
    float2 rCoords = finalCoords + dir * uOpacity;
    float2 bCoords = finalCoords - dir * uOpacity;
    
    float4 color;
    color.r = tex2D(uImage0, rCoords).r;
    color.g = tex2D(uImage0, finalCoords).g;
    color.b = tex2D(uImage0, bCoords).b;
    color.a = 1.0;
    
    
    if (distToGrid < 0.002 * uOpacity)
    {
        if (distToGrid < 0.0005 * uOpacity)
        {
            color.rgb += float3(2.0, 2.0, 2.0) * uOpacity; 
        }
        else
        {
            color.rgb += float3(1.5, 0.1, 1.2) * uOpacity * 2.0; 
        }
    }
    
    return color;
}

technique Technique1
{
    pass MultiSlashPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
