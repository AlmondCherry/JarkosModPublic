sampler uImage0 : register(s0);

float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float uProgress;
float uIntensity;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 diff = uTargetPosition - uScreenPosition;
    float2 center = float2(diff.x / uScreenResolution.x, diff.y / uScreenResolution.y);

    float aspect = uScreenResolution.x / uScreenResolution.y;

    float2 offset = coords - center;
    float2 aspOffset = float2(offset.x * aspect, offset.y);

    float distSq = dot(aspOffset, aspOffset);

    float dist = sqrt(distSq);

    float env = uProgress * (1.0 - uProgress);
    env = env * 4.0;
    env = saturate(env);

    float maxRadius = 0.9;
    float radius = uProgress * maxRadius;

    float ringDelta = dist - radius;
    ringDelta = abs(ringDelta);

    float ringWidth = 14.0;
    float rd = ringDelta * ringWidth;
    float ringFall = rd * rd;
    float ring = 1.0 - ringFall;
    ring = saturate(ring);

    float pushStrength = ring * env;
    pushStrength = pushStrength * uIntensity;
    pushStrength = pushStrength * 0.06;

    float2 distortion = float2(offset.x * pushStrength, offset.y * pushStrength);
    float2 sampleCoords = coords - distortion;
    float4 baseColor = tex2D(uImage0, sampleCoords);

    float flashCore = env * env;

    float centerGlow = 1.0 - distSq;
    centerGlow = saturate(centerGlow);
    centerGlow = centerGlow * 0.4;

    float flashAmount = flashCore + flashCore * centerGlow;
    flashAmount = flashAmount * uIntensity;
    flashAmount = saturate(flashAmount);

    float3 flashColor = float3(0.6, 0.8, 1.0);
    float3 finalColor = lerp(baseColor.rgb, flashColor, flashAmount);

    float crest = ring * env;
    crest = crest * 0.25;
    finalColor.r = finalColor.r + crest;
    finalColor.g = finalColor.g + crest;
    finalColor.b = finalColor.b + crest;
    finalColor = saturate(finalColor);

    float4 result = float4(finalColor, baseColor.a);

    return result * sampleColor;
}

technique Technique1
{
    pass CelestiaTransitionPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
