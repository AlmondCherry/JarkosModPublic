sampler uImage0 : register(s0);
float uTime;
float uIntensity;
float4 uSourceRect;

float hash21(float2 p)
{
    float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
    float dotv = dot(p3, float3(p3.y + 33.33, p3.z + 33.33, p3.x + 33.33));
    p3 = p3 + dotv;
    return frac((p3.x + p3.y) * p3.z);
}

float valueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + float2(1.0, 0.0));
    float c = hash21(i + float2(0.0, 1.0));
    float d = hash21(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float fbm(float2 p)
{
    float total = valueNoise(p) * 0.5;
    p = p * 2.0; total = total + valueNoise(p) * 0.25;
    p = p * 2.0; total = total + valueNoise(p) * 0.125;
    p = p * 2.0; total = total + valueNoise(p) * 0.0625;
    return total;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 localCoords;
    localCoords.x = (coords.x - uSourceRect.x) / uSourceRect.z;
    localCoords.y = (coords.y - uSourceRect.y) / uSourceRect.w;
    localCoords.y = 1.0 - localCoords.y;

    float2 dir = localCoords - float2(0.5, 0.5);
    float2 ovalDir = float2(dir.x * 1.7, dir.y * 0.8);
    float topFactor = saturate(0.5 - dir.y);
    ovalDir.x = ovalDir.x * (1.0 + topFactor * 0.9);
    ovalDir.y = ovalDir.y + dir.y * 0.1;

    float breath = 1.0 + sin(uTime * 1.7) * 0.06 * (0.5 + uIntensity);

    float scroll = uTime * (1.4 + uIntensity * 2.8);
    float n1 = fbm(float2(localCoords.x * 5.0,  localCoords.y * 9.0  - scroll));
    float n2 = fbm(float2(localCoords.x * 11.0 + 3.7, localCoords.y * 16.0 - scroll * 1.6));
    float stream = n1 * 0.7 + n2 * 0.3;

    float warpAmt = 0.18 + uIntensity * 0.24;
    ovalDir = ovalDir + (stream - 0.5) * float2(warpAmt, warpAmt * 1.3);

    float reach = (0.30 + uIntensity * 0.14) * breath;
    float radial = dot(ovalDir, ovalDir) / (reach * reach);

    float jag = (stream - 0.5) * (1.1 + uIntensity * 0.7);
    float body = saturate(1.0 - (radial + jag));
    float bodySharp = saturate(body * body * (1.5 + uIntensity * 0.6));

    float tear = saturate((stream - 0.45) * 2.2);
    float edgeZone = saturate(radial * 1.4 - 0.2);
    bodySharp = saturate(bodySharp - tear * edgeZone * 0.55);

    float glow = saturate(1.0 - radial * 0.9);
    glow = glow * glow * 0.35;

    float streak = 0.5 + stream * 0.85;
    float energy = bodySharp * streak * (0.9 + 0.1 * sin(uTime * (4.0 + uIntensity * 6.0)));

    float innerHole = saturate(1.0 - radial * 3.2);
    innerHole = innerHole * innerHole;
    energy = energy * (1.0 - innerHole * 0.95);

    float2 sparkUV = float2(localCoords.x * 22.0, localCoords.y * 22.0 - scroll * 3.2);
    float sparkRnd = hash21(floor(sparkUV));
    float sparkMask = saturate((sparkRnd - 0.93) * 30.0);
    float2 sparkLocal = frac(sparkUV) - 0.5;
    float sparkDot = saturate(1.0 - dot(sparkLocal, sparkLocal) * 14.0);
    float sparks = sparkMask * sparkDot * sparkDot * bodySharp * (0.6 + uIntensity * 1.4);

    float3 colEdge = float3(0.45, 0.00, 0.55);
    float3 colBody = float3(1.00, 0.25, 0.75);
    float3 colCore = float3(1.00, 0.92, 1.00);

    float coreFall = saturate(1.0 - radial * 6.0);
    float core = coreFall * coreFall;
    core = core * core * bodySharp * (0.6 + uIntensity * 0.9);

    float3 auraColor = lerp(colEdge, colBody, saturate(energy * 1.4));
    auraColor += colCore * core + colCore * sparks + colBody * glow * (0.3 + uIntensity * 0.5);
    auraColor *= (1.0 + uIntensity * 1.2);

    float alpha = saturate(energy + core * 0.6 + sparks + glow * 0.3);
    alpha *= 0.20 + uIntensity * 0.12;

    float4 result;
    result.rgb = auraColor * alpha;
    result.a = alpha;
    return result * sampleColor;
}

technique Technique1
{
    pass P0 { PixelShader = compile ps_3_0 PixelShaderFunction(); }
}