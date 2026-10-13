sampler uImage0 : register(s0);
float4 uColor;
float4 uSecondaryColor;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 p = coords - float2(0.5, 0.5);
    float along = p.x * uDirection.x + p.y * uDirection.y;
    float side = p.x * -uDirection.y + p.y * uDirection.x;
    float band = frac(along * 10.0 + uTime * 4.0);
    float pulse = band * (1.0 - band) * 4.0;
    float mask = 1.0 - saturate(abs(side) * 2.1);
    float split = pulse * mask * uIntensity * 0.010;
    float2 shift;
    shift.x = uDirection.x * split + -uDirection.y * split * 0.6;
    shift.y = uDirection.y * split + uDirection.x * split * 0.6;
    float red = tex2D(uImage0, coords + shift).r;
    float4 baseColor = tex2D(uImage0, coords);
    float blue = tex2D(uImage0, coords - shift).b;
    float flicker = 0.65 + pulse * 0.70;
    float4 color;
    color.r = red * flicker + uColor.r * pulse * uIntensity * 0.12;
    color.g = baseColor.g * (0.75 + pulse * 0.35);
    color.b = blue * flicker + uSecondaryColor.b * pulse * uIntensity * 0.16;
    color.a = baseColor.a * uOpacity * pulse * mask * 0.85;
    return color;
}
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
 