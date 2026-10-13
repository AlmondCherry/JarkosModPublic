




sampler uImage0 : register(s0);

float uShatter;
float uOpacity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float p = saturate(uShatter);
    float p2 = p * p;

    
    float d1 = coords.x * 0.866 + coords.y * 0.5;
    float d2 = coords.y * 0.906 - coords.x * 0.423;
    float c1 = floor(d1 * 5.0);
    float c2 = floor(d2 * 4.0);
    float h0 = frac((c1 * 7.0 + c2 * 13.0) * 0.618034);
    float fall0 = (0.2 + h0 * 1.1) * p2;

    float2 src = coords;
    src.y = src.y - fall0;
    src.x = src.x + (h0 - 0.5) * p2 * 0.18;

    
    float f1 = (src.x * 0.866 + src.y * 0.5) * 5.0;
    float f2 = (src.y * 0.906 - src.x * 0.423) * 4.0;
    float h = frac((floor(f1) * 7.0 + floor(f2) * 13.0) * 0.618034);
    float fall = (0.2 + h * 1.1) * p2;

    float2 src2 = coords;
    src2.y = src2.y - fall;
    src2.x = src2.x + (h - 0.5) * p2 * 0.18;

    float4 shard = tex2D(uImage0, src2);
    float4 orig = tex2D(uImage0, coords);

    
    float g1 = abs(frac(f1) - 0.5);
    float g2 = abs(frac(f2) - 0.5);
    float edgeDist = min(0.5 - g1, 0.5 - g2);
    float crack = saturate(1.0 - edgeDist * 26.0);

    
    float valid = saturate(src2.y * 40.0);
    float shardFade = saturate(1.0 - (p - 0.55) * 2.2);
    float keep = valid * shardFade;

    
    float3 col = lerp(orig.rgb, shard.rgb, keep);

    
    col = col * (1.0 - crack * 0.45 * keep);
    col = col + float3(0.85, 0.9, 1.0) * (crack * keep * saturate(1.0 - p * 2.2) * 0.55);

    
    float rest = saturate(p * 8.0 - 7.0);
    col = lerp(col, orig.rgb, rest);

    return float4(col, 1.0) * uOpacity;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
