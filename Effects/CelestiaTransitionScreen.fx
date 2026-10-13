sampler uImage0 : register(s0);

float uProgress;
float uIntensity;
float2 uTargetPosition;
float2 uScreenPosition;
float2 uScreenResolution;
float uTime;
float3 uColor;
float uOpacity;

float4 TransitionPS(float2 coords : TEXCOORD0) : COLOR0
{
    float2 riftUV = (uTargetPosition - uScreenPosition) / uScreenResolution;
    float2 toRift = riftUV - coords;
    float dist = length(toRift) + 0.0001;
    float2 dirN = toRift / dist;

    float suck = uIntensity * 0.045 / (dist + 0.35) * (1.0 + 0.2 * sin(uTime * 6.0));
    float2 warped = coords + dirN * suck;

    float ripple = sin(dist * 60.0 - uTime * 14.0) * 0.004 * uIntensity;
    warped += dirN * ripple;

    float aber = 0.006 * uIntensity;
    float rC = tex2D(uImage0, warped + dirN * aber).r;
    float gC = tex2D(uImage0, warped).g;
    float bC = tex2D(uImage0, warped - dirN * aber).b;
    float3 col = float3(rC, gC, bC);

    float darken = 1.0 - uIntensity * 0.55;
    float vign = 1.0 - saturate(length(coords - 0.5) * 1.2) * uIntensity * 0.6;
    col *= darken * vign;

    float glow = saturate(1.0 - dist * 2.0) * uIntensity * 0.35;
    col += float3(0.45, 0.15, 0.85) * glow;

    return float4(col, 1.0);
}

technique Technique1
{
    pass TransitionPass
    {
        PixelShader = compile ps_3_0 TransitionPS();
    }
}
