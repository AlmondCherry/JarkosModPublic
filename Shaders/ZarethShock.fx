
sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float uOpacity;
float uIntensity;  
float uProgress;   
float uTime;
float2 uZoom;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float t = uProgress * 20.0;

    
    float shake = sin(t * 13.0) * cos(t * 7.0) * 0.006 * uIntensity;
    float2 uv = coords + float2(shake, 0);

    float4 color = tex2D(uImage0, uv);

    
    float split = 0.004 * uIntensity;
    color.r = tex2D(uImage0, uv + float2(split, 0)).r;
    color.b = tex2D(uImage0, uv - float2(split, 0)).b;

    
    float gray = dot(color.rgb, float3(0.3, 0.59, 0.11));
    color.rgb = lerp(color.rgb, float3(gray, gray, gray), 0.6 * uIntensity);

    
    float pulse = (sin(t * 4.0) * 0.5 + 0.5) * 0.15 * uIntensity;
    color.rgb += pulse;

    
    float2 center = coords - 0.5;
    float vignette = dot(center, center) * 1.8 * uIntensity;
    color.rgb *= 1.0 - vignette;

    return color;
}

technique Technique1
{
    pass ZarethShockPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
