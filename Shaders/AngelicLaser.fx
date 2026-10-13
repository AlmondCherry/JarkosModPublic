



sampler uImage0 : register(s0); 

float3 uColor;          
float3 uSecondaryColor; 
float uOpacity;         
float uTime;            

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    
    float localWidth = lerp(1.0, 0.25, coords.x);

    
    float distFromCenter = abs(coords.y - 0.5) * 2.0;

    
    float distAdjusted = distFromCenter / max(localWidth, 0.01);

    
    float core = exp(-distAdjusted * 8.0);
    float glow = exp(-distAdjusted * 2.5);

    
    float2 noiseUv1 = float2(coords.x * 2.0 - uTime * 3.5, coords.y);
    float2 noiseUv2 = float2(coords.x * 4.0 - uTime * 5.5, coords.y * 1.5);

    float n1 = tex2D(uImage0, noiseUv1).r;
    float n2 = tex2D(uImage0, noiseUv2).r;
    float noise = (n1 + n2) * 0.5;

    
    core *= (0.7 + noise * 0.3);
    glow *= (0.4 + noise * 0.6);

    
    float beamShape = 1.0 - smoothstep(0.0, 1.0, distAdjusted);

    
    float edgeFade = smoothstep(0.0, 0.03, coords.x) * smoothstep(0.0, 0.06, 1.0 - coords.x);

    
    float3 finalColor = uColor * glow + uSecondaryColor * core * 1.5;

    
    float alpha = saturate(beamShape * edgeFade) * uOpacity;

    return float4(finalColor, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
