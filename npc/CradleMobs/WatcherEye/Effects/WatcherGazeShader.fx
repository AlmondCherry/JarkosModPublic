sampler uImage0 : register(s0);

float uOpacity; 
float uProgress; 
float2 uImageOffset; 

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;

    float2 center = float2(0.5, 0.5); 
    float2 p = coords - center;
    p.x *= aspect; 

    float dist = length(p);
    
    
    float time = uProgress * 4.0;
    float wave = sin(dist * 20.0 - time);
    
    float distortion = wave * 0.02 * uOpacity * smoothstep(0.1, 0.8, dist); 
    
    float2 uv = coords + normalize(p) * distortion;
    
    
    float4 baseColor = tex2D(uImage0, uv);
    
    
    float vignette = smoothstep(0.9, 0.35, dist); 
    
    
    float luminance = dot(baseColor.rgb, float3(0.299, 0.587, 0.114));
    float3 curseColor = float3(0.4, 0.05, 0.6); 
    float3 tintColor = lerp(float3(luminance, luminance, luminance), curseColor, 0.6);
    
    
    float3 finalColor = lerp(tintColor, baseColor.rgb, vignette); 
    
    
    finalColor = lerp(baseColor.rgb, finalColor, uOpacity);
    
    
    float darkness = lerp(1.0, vignette * 0.7 + 0.3, uOpacity);
    finalColor *= darkness;

    return float4(finalColor, baseColor.a);
}

technique Technique1
{
    pass WatcherGaze
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
