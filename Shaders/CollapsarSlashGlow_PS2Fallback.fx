sampler uImage0 : register(s0);
float4 uColor;
float4 uSecondaryColor;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords;
    float2 p = uv - float2(0.5, 0.5);
    float tangentX = -uDirection.y;
    float tangentY = uDirection.x;
    float along = p.x * uDirection.x + p.y * uDirection.y;
    float side = p.x * tangentX + p.y * tangentY;
    float sideMask = 1.0 - saturate(abs(side) * 2.35);
    float lengthMask = 1.0 - saturate(abs(along) * 1.25);
    float mask = sideMask * lengthMask;
    float ribPhase = frac(along * 18.0 - uTime * 1.2);
    float rib = ribPhase * (1.0 - ribPhase) * 4.0;
    float wave = (rib - 0.5) * uIntensity * mask;
    float2 offset;
    offset.x = tangentX * wave * 0.040 + uDirection.x * wave * 0.012;
    offset.y = tangentY * wave * 0.040 + uDirection.y * wave * 0.012;
    float4 color = tex2D(uImage0, uv + offset);
    float shade = 0.72 + rib * 0.85 * uIntensity * mask;
    color.r = color.r * shade + uColor.r * rib * uIntensity * mask * 0.18;
    color.g = color.g * shade + uColor.g * rib * uIntensity * mask * 0.18;
    color.b = color.b * (shade + 0.10 * uIntensity * mask) + uSecondaryColor.b * rib * uIntensity * mask * 0.20;
    color.a = color.a * uOpacity * (0.85 + rib * uIntensity * mask * 0.25);
    return color;
}
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
 