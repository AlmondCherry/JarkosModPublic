












sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uTime;
float uIntensity;
float uProgress;
float uDirection;
float uRotation;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageSize0;
float2 uImageSize1;
float3 uLightSource;
float4 uSourceRect;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float intensity = max(uIntensity, 0.0001);

    float2 toCenter = coords - 0.5;
    float dist = length(toCenter);

    
    
    
    float edgeFactor = saturate(dist * 1.55);
    edgeFactor = edgeFactor * edgeFactor * edgeFactor;

    
    float wave = sin(uTime * 1.7 + dist * 22.0);
    float2 edgeWarp = toCenter * edgeFactor * (0.05 + wave * 0.04) * intensity;
    float2 uv = saturate(coords - edgeWarp);

    
    
    float aberration = (0.003 + edgeFactor * 0.015) * intensity;
    float2 aberrOffset = toCenter / max(dist, 0.0001) * aberration;

    float r = tex2D(uImage0, saturate(uv - aberrOffset)).r;
    float4 g = tex2D(uImage0, uv);
    float b = tex2D(uImage0, saturate(uv + aberrOffset)).b;

    float3 color = float3(r, g.g, b);

    
    float vignette = smoothstep(0.45, 0.95, dist);
    color = lerp(color, color * 0.55 + uSecondaryColor * 0.30, vignette * intensity);

    
    
    
    
    color = lerp(g.rgb, color, uOpacity);

    return float4(color, g.a);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
