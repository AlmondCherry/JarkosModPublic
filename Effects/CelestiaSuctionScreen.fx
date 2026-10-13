sampler uImage0 : register(s0);

float uIntensity;
float uProgress;
float2 uTargetPosition;
float2 uScreenPosition;
float2 uScreenResolution;
float uTime;
float3 uColor;
float uOpacity;

float4 SuctionPS(float2 coords : TEXCOORD0) : COLOR0
{
    float aspect = uScreenResolution.x / uScreenResolution.y;
    float2 center = (uTargetPosition - uScreenPosition) / uScreenResolution;

    float eff = uIntensity * saturate(uOpacity);

    float2 toC = coords - center;
    toC.x *= aspect;
    float dist = length(toC) + 0.0001;
    float2 dirN = toC / dist;

    float swirl = eff * 1.6 / (dist + 0.35) * 0.22;
    float cs = cos(swirl), sn = sin(swirl);
    float2 rot = float2(toC.x * cs - toC.y * sn, toC.x * sn + toC.y * cs);

    float pull = eff * 0.20 / (dist + 0.45) * (1.0 + 0.15 * sin(uTime * 7.0));
    float2 warped = rot * (1.0 + pull);
    warped.x /= aspect;
    warped += center;

    float2 blurStep = dirN * float2(1.0 / aspect, 1.0) * pull * 0.35;
    float3 col = tex2D(uImage0, warped).rgb * 0.5;
    col += tex2D(uImage0, warped + blurStep).rgb * 0.3;
    col += tex2D(uImage0, warped + blurStep * 2.0).rgb * 0.2;

    float aber = 0.007 * eff;
    float2 aberDir = dirN * float2(1.0 / aspect, 1.0);
    col.r = tex2D(uImage0, warped + aberDir * aber).r;
    col.b = tex2D(uImage0, warped - aberDir * aber).b;

    float vign = 1.0 - saturate(dist * 1.1) * eff * 0.65;
    col *= (1.0 - eff * 0.35) * vign;
    col += float3(0.45, 0.15, 0.85) * saturate(1.0 - dist * 2.2) * eff * 0.35;

    float3 original = tex2D(uImage0, coords).rgb;
    col = lerp(original, col, saturate(uOpacity));

    return float4(col, 1.0);
}

technique Technique1
{
    pass SuctionPass
    {
        PixelShader = compile ps_3_0 SuctionPS();
    }
}
