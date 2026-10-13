sampler uImage0 : register(s0);

float4x4 uTransform;
float uTime;
float uOpacity;
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

    float n1 = tex2D(uImage0, float2(uv.x * 1.5 - uTime * 1.0, uv.y * 0.6 + 0.21)).r;
    float n2 = tex2D(uImage0, float2(uv.x * 3.2 - uTime * 1.9 + 0.37, uv.y * 1.2 + 0.63)).g;
    float stream = saturate(n1 * 0.8 + n2 * 0.5 - 0.1);

    float across = 1.0 - abs(uv.y * 2.0 - 1.0);
    float edge = pow(saturate(across + stream * 0.4 - 0.12), 1.3);
    float tail = pow(saturate(1.0 - uv.x), 1.3);
    float core = pow(across, 6.0) * (0.8 + stream * 0.8);
    float mote = saturate((tex2D(uImage0, float2(uv.x * 5.0 - uTime * 2.6, uv.y * 2.0 + 0.11)).b - 0.72) * 9.0);
    mote = mote * across * 1.4;

    float body = edge * (0.5 + stream * 1.0);

    float4 col = lerp(uColor, uColorCore, saturate(core + stream * 0.5 + mote));

    float alpha = saturate((body + core + mote)) * input.Color.a * uOpacity;
    return col * alpha;
}

technique Technique1
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexFunction();
        PixelShader = compile ps_2_0 PixelFunction();
    }
}
