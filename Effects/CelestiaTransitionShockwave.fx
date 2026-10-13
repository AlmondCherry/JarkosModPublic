sampler uImage0 : register(s0);
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float uProgress;
float uIntensity;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 diff = uTargetPosition - uScreenPosition;
    float2 center;
    center.x = diff.x / uScreenResolution.x;
    center.y = diff.y / uScreenResolution.y;

    float2 offset = coords - center;
    float distSq = dot(offset, offset);

    float radius = uProgress * 1.4;
    float radiusSq = radius * radius;

    float d = abs(distSq - radiusSq);
    float ringRaw = 1.0 - d * 18.0;
    float ring = (ringRaw + abs(ringRaw)) * 0.5;

    float fade = 1.0 - uProgress;

    float strength = ring * fade * 0.05;
    float2 distortion = offset * strength;

    float4 baseColor = tex2D(uImage0, coords - distortion);

    float3 flashColor = float3(0.35, 0.65, 1.0);
    float flashAmount = fade * fade * uIntensity;

    float3 mixed = lerp(baseColor.rgb, flashColor, flashAmount);

    float4 result = float4(mixed.r, mixed.g, mixed.b, baseColor.a);

    return result * sampleColor;
}

technique Technique1
{
    pass CelestiaTransitionPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}