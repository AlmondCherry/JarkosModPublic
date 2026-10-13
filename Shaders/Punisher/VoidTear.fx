sampler uImage0 : register(s0);

float uTime;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords;
    float2 uCenter = float2(0.5, 0.5);
    
    float2 delta = uv - uCenter;
    delta.x *= 1.777; 
    
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);
    
    
    float line1 = abs(sin(uv.x * 25.0 + uv.y * 35.0));
    float line2 = abs(cos(uv.x * -18.0 + uv.y * 22.0));
    float line3 = abs(sin(angle * 12.0));
    float line4 = abs(cos(dist * 40.0));
    
    float crack = min(min(line1, line2), min(line3, line4));
    float crackSharp = smoothstep(0.02, 0.04, crack);
    
    
    float edgeMask = smoothstep(0.4, 0.8, dist);
    
    
    float2 refractionOffset = float2(sin(uv.y * 50.0), cos(uv.x * 50.0)) * 0.015 * (1.0 - crackSharp);
    float4 screenCol = tex2D(uImage0, uv + refractionOffset * edgeMask * uOpacity);
    
    
    float galaxyWave = sin(angle * 5.0 - dist * 12.0 + uTime * 2.0);
    float3 galaxyCol = float3(0.2, 0.0, 0.6) * galaxyWave + float3(0.1, 0.0, 0.2); 
    
    
    float starNoise = frac(sin(dot(uv + uTime*0.05, float2(12.9898, 78.233))) * 43758.5453);
    if (starNoise > 0.98) galaxyCol += float3(1.0, 1.0, 1.0);
    
    
    float3 glassTint = screenCol.rgb * float3(0.85, 0.85, 1.1) + float3(0.05, 0.0, 0.1) * (1.0 - crackSharp);
    float3 finalRGB = lerp(galaxyCol, glassTint, crackSharp);
    
    float effectStrength = edgeMask * uOpacity;
    float3 outColor = lerp(screenCol.rgb, finalRGB, effectStrength);
    
    outColor -= (dist * 0.5) * effectStrength;
    
    return float4(saturate(outColor), screenCol.a);
}

technique Technique1
{
    pass VoidTearPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
