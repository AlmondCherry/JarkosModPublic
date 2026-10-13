sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float along = centered.x * uDirection.x + centered.y * uDirection.y;

    float stripe = frac(along * 18.0 - uTime * 2.0);
    float rib = 0.65;

    if (stripe > 0.5)
        rib = 1.55;

    float4 color = tex2D(uImage0, coords);
    float power = 1.0 + uIntensity * (rib - 1.0);

    color.r = color.r * power + uSecondaryColor.r * uIntensity * 0.10;
    color.g = color.g * power + uSecondaryColor.g * uIntensity * 0.10;
    color.b = color.b * power + uSecondaryColor.b * uIntensity * 0.10;
    color.a = color.a * uOpacity;

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
