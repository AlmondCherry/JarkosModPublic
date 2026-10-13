sampler uImage0 : register(s0);
 
float3 uSecondaryColor;
float uOpacity;
float4 uShaderSpecificData;
 
float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float4 tex = tex2D(uImage0, coords);
 
    float edgeCut = uShaderSpecificData.x;
    float coreSize = uShaderSpecificData.y;
    float coreBoost = uShaderSpecificData.z;
 
    float2 centered = abs(coords - 0.5f) * 2.0f;
    float d = max(centered.x, centered.y);
 
    clip(1.0f - edgeCut - d);
 
    float coreMask = step(d, coreSize);
    float3 color = saturate(vertexColor.rgb + uSecondaryColor * coreMask * coreBoost);
 
    return float4(color, tex.a * vertexColor.a * uOpacity);
}
 
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}