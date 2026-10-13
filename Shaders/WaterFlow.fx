



sampler uImage0 : register(s0);

float uProgress;
float2 uTargetPosition;

float4 WaterFlowTextPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, coords) * sampleColor;
}

float4 GroundPulsePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 src = tex2D(uImage0, coords);
    float dx = coords.x - uTargetPosition.x;
    float dy = coords.y - uTargetPosition.y;
    float distSq = dx * dx + dy * dy;
    float r = uProgress * 0.3;
    float dr = distSq - r;
    float ring = saturate(1.0 - dr * dr * 80.0);
    src.r = src.r + ring;
    return src;
}

technique Technique1
{
    pass WaterFlowText
    {
        PixelShader = compile ps_2_0 WaterFlowTextPS();
    }
    pass GroundPulse
    {
        PixelShader = compile ps_2_0 GroundPulsePS();
    }
}
