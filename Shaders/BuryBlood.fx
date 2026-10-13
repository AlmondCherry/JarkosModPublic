sampler2D TextureSampler : register(s0);
float time;

float4 MainPS(float2 uv : TEXCOORD0) : COLOR
{
    float wave = sin(uv.y * 20 + time * 6) * 0.03;
    uv.x += wave;

    float4 col = tex2D(TextureSampler, uv);

    col.rgb *= float3(0.6, 0, 0);
    col.rgb += sin(time * 5) * 0.1;

    return col;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}