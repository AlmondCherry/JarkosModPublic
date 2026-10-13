sampler uImage0 : register(s0);
float uOpacity; 

float2 uTearNormal; 
float2 uTearPoint;  

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    if (uOpacity <= 0.0) return tex2D(uImage0, coords);

    float distanceToLine = dot(coords - uTearPoint, uTearNormal);
    
    
    float jaggedness = sin(coords.x * 100.0 + coords.y * 100.0) * 0.001 * uOpacity;
    distanceToLine += jaggedness;
    
    
    float distortionForce = 0.05 * uOpacity;
    float2 disp = uTearNormal * distortionForce;
    
    float2 finalCoords;
    if (distanceToLine > 0.0)
    {
        finalCoords = coords + disp;
    }
    else
    {
        finalCoords = coords - disp;
    }
    
    
    float2 dir = float2(-uTearNormal.y, uTearNormal.x);
    float2 rCoords = finalCoords + dir * 0.02 * uOpacity;
    float2 bCoords = finalCoords - dir * 0.02 * uOpacity;
    
    float4 color;
    color.r = tex2D(uImage0, rCoords).r;
    color.g = tex2D(uImage0, finalCoords).g;
    color.b = tex2D(uImage0, bCoords).b;
    color.a = 1.0;
    
    
    float coreDist = abs(distanceToLine);
    if (coreDist < 0.01 * uOpacity)
    {
        if (coreDist < 0.003 * uOpacity)
        {
            color.rgb += float3(2.0, 2.0, 2.0) * uOpacity; 
        }
        else
        {
            color.rgb += float3(1.5, 0.0, 0.0) * uOpacity * 2.0; 
        }
    }
    
    
    if (uOpacity > 0.8)
    {
        color.rgb += float3(1.0, 0.5, 0.5) * (uOpacity - 0.8) * 5.0; 
    }
    
    return color;
}

technique Technique1
{
    pass Phase2SlashPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
