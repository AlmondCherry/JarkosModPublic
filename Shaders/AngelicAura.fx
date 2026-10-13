


sampler uImage0 : register(s0);

float3 uColor;          
float uOpacity;         
float uTime;            
float uProgress;        

float Saturate01(float v)
{
    return min(1.0, max(0.0, v));
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 uv = (coords - 0.5) * 2.0;
    float r = length(uv);

    
    float angle = atan2(uv.y, uv.x);
    float2 noiseUV1 = float2(angle / 6.28318 + uTime * 0.15, r * 0.5 - uTime * 0.12);
    float2 noiseUV2 = coords * 2.0 + float2(-uTime * 0.1, uTime * 0.15);

    float n1 = tex2D(uImage0, noiseUV1).r;
    float n2 = tex2D(uImage0, noiseUV2).r;
    float noise = (n1 * 0.6 + n2 * 0.4);

    
    
    
    float outerRadius = 0.95;
    float innerRadius = 0.82 - (noise - 0.5) * 0.035; 

    
    if (r > outerRadius || r < innerRadius - 0.03)
        return float4(0, 0, 0, 0);

    
    float whiteBorderDist = abs(r - outerRadius);
    float whiteOutline = smoothstep(0.018, 0.001, whiteBorderDist);
    float3 outlineColor = float3(1.0, 1.0, 1.0) * whiteOutline * 2.5;

    
    
    float innerDist = r - innerRadius;
    float bandWidth = outerRadius - innerRadius;
    float bandNormalized = saturate(innerDist / bandWidth);
    
    
    float yellowSoftGlow = sin(bandNormalized * 3.14159);
    yellowSoftGlow = pow(yellowSoftGlow, 1.2); 

    
    float wavePulse = 0.8 + 0.4 * sin(angle * 10.0 + uTime * 6.0 + noise * 5.0);
    float3 yellowColor = uColor * yellowSoftGlow * 2.5 * wavePulse;

    
    float3 finalColor = yellowColor + outlineColor;

    
    float alpha = Saturate01(yellowSoftGlow * 0.95 + whiteOutline * 0.98) * uOpacity;

    return float4(finalColor * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
