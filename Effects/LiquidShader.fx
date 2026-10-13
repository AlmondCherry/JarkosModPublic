sampler uImage0 : register(s0);
float uTime;
float2 uUvOffset;
float2 uUvScale;

float hash(float2 p) {
    p = frac(p * 0.3183099 + 0.1);
    p *= 17.0;
    return frac(p.x * p.y * (p.x + p.y));
}

float bubble(float2 uv, float2 center, float radius) {
    float d = length(uv - center);
    float b = smoothstep(radius, radius - 0.02, d);
    float hollow = smoothstep(radius * 0.7, radius, d);
    return b * (hollow + 0.3); 
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 globalUv = coords * uUvScale + uUvOffset;
    float2 uv = globalUv;
    uv.y *= 5.0;
    
    float bubbleMask = 0.0;
    
    for(int i = 0; i < 4; i++) {
        float fi = (float)i;
        float speed = 1.0 + fi * 0.4;
        float2 scroll = float2(0.0, uTime * speed);
        
        float2 gridUV = frac(uv * (2.0 + fi) - scroll);
        float2 gridID = floor(uv * (2.0 + fi) - scroll);
        
        float h = hash(gridID + fi * 15.0);
        float2 center = float2(h, frac(h * 43.0));
        
        float radius = 0.15 + h * 0.25; 
        bubbleMask += bubble(gridUV, center, radius) * (0.6 + h * 0.4);
    }
    
    float4 finalColor = color;

    finalColor.rgb *= (0.6 + 0.4 * sin(globalUv.x * 3.14159));
    finalColor.rgb *= (0.4 + 0.6 * (1.0 - globalUv.y));
    finalColor.rgb += bubbleMask * 0.9 * float3(1, 1, 1);
    
    finalColor.a = color.a;
    return finalColor * tex2D(uImage0, coords);
}

technique Technique1
{
    pass LiquidPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
