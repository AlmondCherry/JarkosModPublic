sampler uImage0 : register(s0);
float uOpacity; 

float2 uTearNormal; 
float2 uTearPoint;  

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    if (uOpacity <= 0.0) return tex2D(uImage0, coords);

    float mainDistance = dot(coords - uTearPoint, uTearNormal);
    float jaggedness = sin(coords.x * 50.0 + coords.y * 50.0) * 0.003 * uOpacity;
    
    
    float lineSpacing = 0.04; 
    float relativePos = mainDistance / lineSpacing;
    float nearestLineIndex = round(relativePos);
    
    float2 finalCoords = coords;
    float4 color = tex2D(uImage0, coords);
    
    
    if (abs(nearestLineIndex) <= 4.0)
    {
        float distToNearestLine = abs(mainDistance - nearestLineIndex * lineSpacing) + jaggedness;
        
        
        float2 dir = float2(-uTearNormal.y, uTearNormal.x);
        
        
        if (distToNearestLine < 0.01 * uOpacity)
        {
            float distortionForce = 0.02 * uOpacity * sign(nearestLineIndex + 0.1);
            float2 disp = uTearNormal * distortionForce;
            finalCoords = coords + disp;
            
            float2 rCoords = finalCoords + dir * 0.01 * uOpacity;
            float2 bCoords = finalCoords - dir * 0.01 * uOpacity;
            
            color.r = tex2D(uImage0, rCoords).r;
            color.g = tex2D(uImage0, finalCoords).g;
            color.b = tex2D(uImage0, bCoords).b;
            color.a = 1.0;
        }
        
        
        if (distToNearestLine < 0.004 * uOpacity)
        {
            if (distToNearestLine < 0.0015 * uOpacity)
            {
                color.rgb += float3(2.0, 2.0, 2.0) * uOpacity; 
            }
            else
            {
                color.rgb += float3(1.5, 0.1, 1.2) * uOpacity * 2.0; 
            }
        }
    }
    
    
    if (uOpacity > 0.8)
    {
        color.rgb += float3(1.0, 1.0, 1.0) * (uOpacity - 0.8) * 3.0; 
    }
    
    return color;
}

technique Technique1
{
    pass UniverseTearPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
