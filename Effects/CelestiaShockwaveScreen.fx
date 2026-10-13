sampler uImage0 : register(s0);

float uProgress;
float uIntensity;
float2 uTargetPosition;
float2 uScreenPosition;
float2 uScreenResolution;
float uTime;
float3 uColor;
float uOpacity;

float4 ShockwavePS(float2 coords : TEXCOORD0) : COLOR0
{
    float aspect = uScreenResolution.x / uScreenResolution.y;
    float2 center = (uTargetPosition - uScreenPosition) / uScreenResolution;

    float eff = uIntensity * saturate(uOpacity);

    float2 toC = coords - center;
    toC.x *= aspect;
    float dist = length(toC) + 0.0001;
    float2 dirN = toC / dist;

    float waveR = uProgress * 1.6;
    float d = dist - waveR;

    float band = exp(-d * d * 90.0);
    float displace = band * 0.10 * eff;
    float2 warped = coords - dirN * float2(1.0 / aspect, 1.0) * displace * sign(d + 0.001);

    float aber = band * 0.018 * eff;
    float2 aberDir = dirN * float2(1.0 / aspect, 1.0);
    float3 col;
    col.r = tex2D(uImage0, warped + aberDir * aber).r;
    col.g = tex2D(uImage0, warped).g;
    col.b = tex2D(uImage0, warped - aberDir * aber).b;

    float invert = band * saturate(eff * 1.5) * 0.8;
    col = lerp(col, 1.0 - col, invert);

    col += float3(0.75, 0.85, 1.0) * band * eff * 0.9;
    col += float3(1.0, 0.98, 1.0) * pow(saturate(1.0 - dist * 2.5), 2.0) * eff;

    float3 original = tex2D(uImage0, coords).rgb;
    col = lerp(original, col, saturate(uOpacity));

    return float4(col, 1.0);
}

technique Technique1
{
    pass ShockwavePass
    {
        PixelShader = compile ps_3_0 ShockwavePS();
    }
}
