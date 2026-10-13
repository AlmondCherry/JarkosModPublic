sampler uImage0 : register(s0);
float3 uColor;
float uOpacity;
float uTime;

float4 MarkFrag(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0 {
    float4 tex = tex2D(uImage0, coords);
    if (tex.a < 0.1)
        return float4(0, 0, 0, 0);

    
    float scan = 0.5 + 0.5 * sin(coords.y * 120.0 + uTime * 10.0);
    return float4(uColor * scan, 0) * uOpacity * tex.a;
}

technique Technique1 {
    pass MarkPass {
        PixelShader = compile ps_2_0 MarkFrag();
    }
}