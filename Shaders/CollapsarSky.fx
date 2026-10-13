





sampler uImage0 : register(s0);

float uIntensity;     
float uFlash;         
float uTime;
float3 uColor;        
float2 uResolution;


float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1, 0));
    float c = hash(i + float2(0, 1));
    float d = hash(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 uv = coords * float2(uResolution.x / uResolution.y, 1.0);

    
    float clouds = noise(uv * 3.0 + float2(uTime * 0.15, 0.0)) * 0.6
                 + noise(uv * 7.0 - float2(uTime * 0.28, uTime * 0.05)) * 0.4;

    
    float grad = lerp(0.95, 0.35, coords.y);

    
    float darkness = saturate(grad + clouds * 0.25) * uIntensity;

    
    float flashGlow = uFlash * (0.35 + clouds * 0.65) * lerp(1.2, 0.4, coords.y);

    
    float3 col = uColor * (0.6 + clouds * 0.5);              
    col += float3(0.75, 0.85, 1.0) * flashGlow;              

    
    
    float alpha = saturate(darkness + flashGlow * 0.5);
    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass SkyPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
