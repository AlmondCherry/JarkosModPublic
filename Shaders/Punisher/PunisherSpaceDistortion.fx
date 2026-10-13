sampler uImage0 : register(s0);
float uTime;
float uOpacity; 
float2 uTearPoint; 
float2 uTearNormal; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    if (uOpacity <= 0.0)
        return tex2D(uImage0, coords);
        
    
    float2 distVec = coords - uTearPoint;
    
    distVec.x *= 1.777;
    float dist = length(distVec);
    
    
    float wave = sin(dist * 30.0 - uTime * 40.0) * exp(-dist * 6.0);
    float distortionForce = wave * uOpacity * 0.08;
    
    
    float2 distortedCoords = coords + uTearNormal * distortionForce;
    
    
    float aberration = distortionForce * 0.4;
    float r = tex2D(uImage0, distortedCoords + float2(aberration, 0.0)).r;
    float g = tex2D(uImage0, distortedCoords).g;
    float b = tex2D(uImage0, distortedCoords - float2(aberration, 0.0)).b;
    
    float4 finalColor = float4(r, g, b, 1.0);
    
    
    finalColor.rgb *= lerp(1.0, 0.6, exp(-dist * 10.0) * uOpacity);
    
    return finalColor;
}

technique Technique1
{
    pass SpaceDistortionPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
