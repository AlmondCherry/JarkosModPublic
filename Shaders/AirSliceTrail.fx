sampler uImage0 : register(s0);

float4x4 uTransform;   
float uTime;           
float uFade;           
float4 uColor;         

struct VSInput
{
    float2 Pos   : POSITION0;
    float4 Color : COLOR0;
    float2 UV    : TEXCOORD0;
};

struct VSOutput
{
    float4 Pos   : POSITION0;
    float4 Color : COLOR0;
    float2 UV    : TEXCOORD0;
};

VSOutput VertexFunction(VSInput input)
{
    VSOutput output;
    output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}

float4 PixelFunction(VSOutput input) : COLOR0
{
    float2 uv = input.UV;

    
    float n1 = tex2D(uImage0, float2(uv.x * 1.8 - uTime * 1.6, uv.y * 0.7)).r;
    float n2 = tex2D(uImage0, float2(uv.x * 3.6 - uTime * 2.4 + 0.37, uv.y * 1.5 + 0.21)).r;
    float ripple = n1 * 0.65 + n2 * 0.35;

    
    float across = 1.0 - abs(uv.y * 2.0 - 1.0);
    float edge = pow(across, 1.5 + ripple * 2.5);

    
    float lengthFade = pow(saturate(1.0 - uv.x), 1.3);

    
    float core = pow(across, 8.0) * (1.2 + ripple * 0.6);

    
    float haze = smoothstep(0.15, 0.9, edge) * 0.30;

    float alpha = (haze + core) * lengthFade * uFade * input.Color.a;

    
    float3 col = uColor.rgb + ripple * 0.20;

    return float4(col * alpha, 0); 
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexFunction();
        PixelShader = compile ps_3_0 PixelFunction();
    }
}
