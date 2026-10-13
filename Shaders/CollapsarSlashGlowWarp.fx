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
    float tangentX = -uDirection.y;
    float tangentY = uDirection.x;
    float along = p.x * uDirection.x + p.y * uDirection.y;
    float side = p.x * tangentX + p.y * tangentY;
    float mask = 1.0 - saturate(abs(side) * 2.45);
    mask *= 1.0 - saturate(abs(along) * 1.30);
    float phase = frac(along * 14.0 + side * 5.0 - uTime * 1.4);
    float wave = (phase * (1.0 - phase) * 4.0 - 0.5) * uIntensity * mask;
    float2 offset;
    offset.x = tangentX * wave * 0.060 + uDirection.x * wave * 0.018;
    offset.y = tangentY * wave * 0.060 + uDirection.y * wave * 0.018;
    float4 color = tex2D(uImage0, coords + offset);
    float glow = 0.85 + mask * uIntensity * 0.35;
    color.r = color.r * glow + uColor.r * mask * uIntensity * 0.08;
    color.g = color.g * glow + uColor.g * mask * uIntensity * 0.08;
    color.b = color.b * glow + uSecondaryColor.b * mask * uIntensity * 0.10;
    color.a = color.a * uOpacity * mask;
    return color;
}
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
 