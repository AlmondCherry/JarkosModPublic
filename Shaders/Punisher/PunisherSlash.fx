matrix TransformMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, TransformMatrix);
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    
    
    float widthFactor = pow(sin(input.TexCoords.y * 3.14159), 2.0);
    
    
    float lengthFactor = input.TexCoords.x; 
    
    float intensity = widthFactor * lengthFactor;
    
    float4 finalColor = input.Color;
    
    
    finalColor.rgb *= (1.0 + intensity * 3.0);
    finalColor.a *= intensity;
    
    return finalColor;
}

technique Technique1
{
    pass SlashPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
