sampler uImage0 : register(s0);

float uIntensity;
float uTime;
float2 uScreenResolution;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, coords);
    float3 col = base.rgb;

    float lum = dot(col, float3(0.299, 0.587, 0.114));

    float3 graded = lerp(col, col * float3(0.82, 0.93, 1.06), 0.30);

    float shadowMask = (1.0 - lum) * (1.0 - lum);
    graded = lerp(graded, float3(0.20, 0.38, 0.80) * (lum + 0.10), shadowMask * 0.12);
    graded += float3(0.008, 0.02, 0.045) * (1.0 - lum);
    graded *= 1.0 + sin(uTime * 1.5) * 0.01;

    float2 v = coords - 0.5;
    float vig = saturate(dot(v, v) * 1.4);
    graded = lerp(graded, float3(0.08, 0.15, 0.32), vig * 0.12);

    float3 finalColor = lerp(col, saturate(graded), saturate(uIntensity));
    return float4(finalColor, base.a) * sampleColor;
}

technique Technique1
{
    pass CelestiaBlueTintPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
