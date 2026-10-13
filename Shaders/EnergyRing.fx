







sampler uImage0 : register(s0);

float uProgress;
float uTime;
float4 uColor;      
float4 uColorCore;  

float4 PixelFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 p = uv * 2.0 - 1.0;
    float r = length(p);
    float ang = atan2(p.y, p.x) / 6.28318 + 0.5;

    
    float n = tex2D(uImage0, float2(ang * 3.0 + uTime * 0.25, uProgress * 0.6)).r;

    
    float ringR = 0.80 + (n - 0.5) * 0.10;

    float band = abs(r - ringR);

    
    float ring = smoothstep(0.085, 0.0, band);
    float glow = smoothstep(0.30, 0.0, band) * 0.45;

    
    float flash = smoothstep(0.6, 0.0, r) * saturate(1.0 - uProgress * 3.0) * 0.8;

    
    float fade = 1.0 - uProgress;
    fade = fade * fade * (3.0 - 2.0 * fade);

    float alpha = (ring * 1.35 + glow + flash) * fade;

    
    alpha *= step(r, 1.0);

    float3 col = lerp(uColor.rgb, uColorCore.rgb, ring * 0.85);

    return float4(col * alpha, 0); 
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelFunction();
    }
}
