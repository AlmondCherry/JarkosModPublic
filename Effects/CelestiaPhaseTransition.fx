sampler uImage0 : register(s0);

float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float uProgress;
float uIntensity;
float uTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 diff = uTargetPosition - uScreenPosition;
    float2 center = float2(diff.x / uScreenResolution.x, diff.y / uScreenResolution.y);

    float aspect = uScreenResolution.x / uScreenResolution.y;
    float2 offset = coords - center;
    float2 aspOffset = float2(offset.x * aspect, offset.y);
    float dist = sqrt(dot(aspOffset, aspOffset));

    float p = uProgress;

    float darkPhase = saturate(p / 0.45);
    float darkRelease = saturate((p - 0.45) * 12.0);
    float darkness = darkPhase * (1.0 - darkRelease);

    float pull = saturate(dist * 1.6);
    float darken = darkness * (0.35 + pull * 0.55);

    float tremor = sin(dist * 60.0 - uTime * 25.0) * 0.004 * darkness;
    float2 implodeUV = coords + offset * (-0.03 * darkness) + float2(tremor, tremor);

    float wp = saturate((p - 0.5) * 2.0);
    float radius = wp * 1.5;
    float ringDelta = abs(dist - radius);
    float rd = ringDelta * 10.0;
    float ring = saturate(1.0 - rd * rd);
    float waveFade = 1.0 - wp;

    float push = ring * waveFade * uIntensity * 0.09;
    float2 distortion = offset * push;

    float2 uv = implodeUV - distortion;

    float ca = ring * waveFade * 0.012;
    float4 colR = tex2D(uImage0, uv + offset * ca);
    float4 colG = tex2D(uImage0, uv);
    float4 colB = tex2D(uImage0, uv - offset * ca);
    float3 baseColor = float3(colR.r, colG.g, colB.b);

    baseColor = baseColor * (1.0 - darken);

    float veins = sin(dist * 40.0 - uTime * 18.0) * 0.5 + 0.5;
    veins = veins * veins * veins;
    float veinGlow = veins * darkness * saturate(1.0 - dist * 1.2) * 0.35;
    baseColor += float3(1.0, 0.35, 0.9) * veinGlow;

    float flashT = 1.0 - abs(p - 0.5) * 10.0;
    flashT = saturate(flashT);
    float flash = flashT * flashT * uIntensity;
    float coreBoost = saturate(1.0 - dist * 0.8) * 0.6;
    float flashAmount = saturate(flash + flash * coreBoost);
    float3 flashColor = float3(1.0, 0.85, 0.98);

    float3 finalColor = lerp(baseColor, flashColor, flashAmount);

    float crest = ring * waveFade;
    finalColor += float3(1.0, 0.4, 0.85) * crest * 0.35;


    float tint = wp * (1.0 - wp) * 0.25;
    finalColor = lerp(finalColor, finalColor * float3(1.08, 0.95, 1.1), tint);

    finalColor = saturate(finalColor);
    return float4(finalColor, colG.a) * sampleColor;
}

technique Technique1
{
    pass CelestiaPhasePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
