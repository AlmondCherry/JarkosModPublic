


















sampler uImage0 : register(s0);

float  uTime;            
float  uOpacity;         
float3 uColor;           
float3 uSecondaryColor;  
float  uSegmentLength;   
float  uThickness;       
float  uProgress;        

float4 ZodiacFlowPS(float2 coords : TEXCOORD0) : COLOR0
{
    float along  = saturate(coords.x);
    float across = saturate(coords.y);

    
    float edge  = 1.0 - abs(across - 0.5) * 2.0;
    edge        = saturate(edge);
    float band  = pow(edge, 1.4);          
    float core  = pow(edge, 9.0);          

    
    float density = max(8.0, uSegmentLength / 14.0);

    
    float scroll1 = sin(along * density        - uTime * 0.35);
    float scroll2 = sin(along * density * 0.55 - uTime * 0.22 + 1.3);
    float streak  = saturate((scroll1 + scroll2) * 0.45 + 0.50);

    
    
    float sharp = saturate(pow(streak, 3.0) * 1.8);

    
    float head = 1.0 - saturate(abs(along - uProgress) * 8.0);
    head = pow(head, 2.2);

    
    float3 col = lerp(uSecondaryColor, uColor, sharp * band);
    col += core * 1.20;             
    col += head * uColor * 0.85;    
    col = saturate(col);

    
    float alpha = band * (0.32 + 0.68 * sharp);
    alpha = saturate(alpha + core * 0.45 + head * 0.6);
    alpha *= uOpacity;

    return float4(col * alpha, alpha);   
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 ZodiacFlowPS();
    }
}
