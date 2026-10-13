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

float uProgress; 

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float x = input.TexCoords.x;
    float y = input.TexCoords.y;
    
    
    float edgeStart = saturate(x * 10.0);
    float edgeEnd = saturate((1.0 - x) * 10.0);
    float edge = min(edgeStart, edgeEnd);
    
    
    float distY = abs(y - 0.5) * 2.0;
    float thickness = max(0.0, 1.0 - distY);
    
    
    thickness *= edge;
    
    float4 finalColor;
    
    
    finalColor.rgb = float3(0.6, 0.0, 1.0) * thickness * uProgress * 2.0; 
    
    
    float core = max(0.0, thickness - 0.6);
    finalColor.rgb += float3(1.0, 1.0, 1.0) * core * uProgress * 5.0;
    
    finalColor.a = thickness * uProgress;
    
    return finalColor * input.Color.a;
}

technique Technique1
{
    pass MiniSlashPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
