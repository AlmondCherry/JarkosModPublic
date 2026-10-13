sampler uImage0 : register(s0);

float4x4 uTransform;
float uTime;
float4 uColor;
float4 uColorCore;

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

    float n = tex2D(uImage0, float2(uv.x * 2.5 - uTime * 3.0, uv.y * 1.2)).r;

    float across = 1.0 - abs(uv.y * 2.0 - 1.0);

    
    float core = pow(across, 5.0) * (0.7 + 0.5 * n);
    float body = pow(across, 1.6) * 0.55;

    
    float tailMask = pow(saturate(1.0 - uv.x), 0.9);

    
    float erosion = smoothstep(uv.x - 0.15, uv.x + 0.25, n + 0.45);

    float alpha = (core + body) * tailMask * erosion * input.Color.a;

    float3 col = lerp(uColor.rgb, uColorCore.rgb, saturate(core));

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
