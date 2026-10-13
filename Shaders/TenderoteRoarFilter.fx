sampler uImage0 : register(s0);

float uIntensity; 
float uProgress;  
float2 uScreenResolution;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 dir = coords - float2(0.5, 0.5);
    float dist = length(dir);
    float2 offsetR = dir * (0.02 * uProgress);
    float2 offsetB = -dir * (0.02 * uProgress);
    
    
    float r = tex2D(uImage0, coords + offsetR).r;
    float g = tex2D(uImage0, coords).g;
    float b = tex2D(uImage0, coords + offsetB).b;
    
    float4 col = float4(r, g, b, 1.0);
    
    
    float3 inverted = float3(1.0, 1.0, 1.0) - col.rgb;
    col.rgb = lerp(col.rgb, inverted, uIntensity);
    
    
    col.rgb += uProgress * 0.15;
    
    return col;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
