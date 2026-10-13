sampler uImage0 : register(s0);

float3 uColor;
float uOpacity; 
float2 uScreenRes; 
float2 uScreenPos; 

float2 uTearPosition; 
float2 uTearDir; 
float uTime; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 worldPos = uScreenPos + coords * uScreenRes;
    
    float2 normal = float2(-uTearDir.y, uTearDir.x);
    float dist = dot(worldPos - uTearPosition, normal);
    float absDist = abs(dist);
    
    if (uOpacity <= 0.0 || absDist > 400.0) return tex2D(uImage0, coords);
    
    
    float jagged = sin(dot(worldPos, uTearDir) * 0.02 + uTime * 10.0) * 15.0;
    jagged += cos(dot(worldPos, uTearDir) * 0.05 - uTime * 15.0) * 5.0; 
    
    float distortedDist = abs(absDist + jagged * uOpacity);
    
    
    float coreWidth = (10.0 + sin(uTime * 20.0) * 3.0) * uOpacity;
    float glowWidth = 100.0 * uOpacity;
    
    float2 disp = normal * 15.0 * uOpacity / uScreenRes;
    float2 displacedCoords = coords + (dist > 0 ? disp : -disp);
    
    
    float4 baseColor;
    baseColor.r = tex2D(uImage0, saturate(displacedCoords + normal * 0.005 * uOpacity)).r;
    baseColor.g = tex2D(uImage0, saturate(displacedCoords)).g;
    baseColor.b = tex2D(uImage0, saturate(displacedCoords - normal * 0.005 * uOpacity)).b;
    baseColor.a = 1.0;
    
    if (distortedDist < coreWidth)
    {
        
        return baseColor + float4(1.5, 0.8, 1.5, 0) * uOpacity;
    }
    
    if (distortedDist < glowWidth)
    {
        float glowAmount = 1.0 - (distortedDist / glowWidth);
        glowAmount = glowAmount * glowAmount * glowAmount; 
        return baseColor + float4(uColor * glowAmount * 2.0 * uOpacity, 0);
    }
    
    return tex2D(uImage0, coords);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
