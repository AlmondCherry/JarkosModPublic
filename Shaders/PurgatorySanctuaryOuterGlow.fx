







sampler uImage0 : register(s0);

float3 uColor;          
float3 uSecondaryColor; 
float uOpacity;         
float uSaturation;      

float4 PixelFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 p = (uv - 0.5) * 2.0;
    float r = length(p);
    float angle = atan2(p.y, p.x) / 6.2831853 + 0.5;

    
    float wave = sin(angle * 16.0 + uSaturation * 4.0) * 0.01 + cos(angle * 8.0 - uSaturation * 2.0) * 0.008;
    float r_edge = 0.65 + wave;

    
    float insideMask = step(r_edge, r);

    
    float distOutside = r - r_edge;
    float glowWidth = 0.98 - r_edge;
    float outerGlow = smoothstep(glowWidth, 0.0, distOutside);

    
    float edgeLine = smoothstep(0.035, 0.0, abs(r - r_edge)) * 1.8;

    
    float intensity = insideMask * (outerGlow * 0.85 + edgeLine);

    float alpha = intensity * saturate(uOpacity);

    float3 col = lerp(uColor, uSecondaryColor, edgeLine * 0.5);

    return float4(col * alpha, 0.0);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelFunction();
    }
}
