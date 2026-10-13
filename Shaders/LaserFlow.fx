



















sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float4 uShaderSpecificData;

float Smooth01(float v)
{
    v = saturate(v);
    return v * v * (3.0 - 2.0 * v);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float along  = coords.y;
    float across = coords.x;

    float speed        = uShaderSpecificData.x;
    float patternScale = max(uShaderSpecificData.y, 0.0001);
    float intensity    = max(uSaturation, 0.0);

    
    float4 baseTex = tex2D(uImage0, coords);
    float mask = baseTex.a;

    
    float scroll = along * patternScale - uTime * speed;

    
    float w1 = sin(scroll * 18.0 + across * 6.2831) * 0.5 + 0.5;
    float w2 = sin(scroll * 7.0  - across * 12.566) * 0.5 + 0.5;
    float w3 = sin(scroll * 2.5  + across * 3.0)    * 0.5 + 0.5;
    float wave = saturate(w1 * 0.5 + w2 * 0.3 + w3 * 0.2);

    
    float edge = 1.0 - abs(across - 0.5) * 2.0;
    edge = saturate(edge);
    float edgeSoft = edge * edge;

    
    float core = pow(edge, 6.0);

    
    float3 col = lerp(uSecondaryColor, uColor, Smooth01(wave + core * 0.5));
    col += uColor * core * 0.35;

    float alpha = mask * edgeSoft * uOpacity * intensity;

    
    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
