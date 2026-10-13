sampler uImage0 : register(s0); 
sampler uImage1 : register(s1); 

float3 uColor;          
float uOpacity;         
float uIntensity;       
float uProgress;        
float2 uImageOffset;    
float2 uScreenPosition; 
float2 uTargetPosition; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;

    
    float2 center = (uTargetPosition - uScreenPosition) / screenSize;

    
    float2 p = coords - center;
    p.x *= aspect;

    float dist = length(p);

    
    
    float auraRadius = uIntensity / screenSize.y;

    
    float softBorder = 40.0 / screenSize.y;

    
    float auraMask = saturate((auraRadius - dist) / softBorder);

    
    
    float2 noiseUV1 = coords * 1.5 + float2(uProgress * 0.04, uProgress * 0.06);
    float2 noiseUV2 = coords * 2.2 - float2(uProgress * 0.05, -uProgress * 0.03);

    float2 n1 = tex2D(uImage1, noiseUV1).rg * 2.0 - 1.0;
    float2 n2 = tex2D(uImage1, noiseUV2).rg * 2.0 - 1.0;

    
    float2 warpOffset = (n1 * 0.6 + n2 * 0.4) * 0.0055;

    
    warpOffset *= auraMask * uOpacity;

    
    float3 sceneColor = tex2D(uImage0, coords + warpOffset).rgb;

    
    
    float ringMask = 1.0 - saturate(abs(dist - auraRadius) / (softBorder * 0.5));
    ringMask = pow(ringMask, 2.0); 

    
    float innerGlow = saturate((auraRadius - dist) / (softBorder * 2.0)) * (1.0 - auraMask);
    
    float3 glow = uColor * (ringMask * 0.9 + innerGlow * 0.25) * uOpacity;

    float3 finalColor = sceneColor + glow;
    return float4(finalColor, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
