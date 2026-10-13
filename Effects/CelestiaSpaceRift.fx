sampler uImage0 : register(s0);

float uTime;
float uSeed;
float uProgress;
float uDetonate;

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
    p = p * 2.03; total += valueNoise(p) * 0.25;
    p = p * 2.01; total += valueNoise(p) * 0.125;
    return total * 1.1428;
}

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 dir = coords - float2(0.5, 0.5);
    float dist = sqrt(dot(dir, dir)) * 2.0;
    float angle = atan2(dir.y, dir.x);
    float t = uTime;

    float holeR = 0.10 + uProgress * 0.16;
    float diskR = holeR + 0.05 + uProgress * 0.30;

    float swirl = angle + 6.0 / (dist + 0.15) + t * 2.5;
    float2 swirlUV = float2(cos(swirl), sin(swirl)) * dist * 3.0;
    float plasma = fbm(swirlUV + uSeed + t * 0.4);

    float diskMask = smoothstep(holeR, holeR + 0.06, dist) * (1.0 - smoothstep(diskR, diskR + 0.25, dist));
    float disk = diskMask * (0.4 + plasma * 0.9);

    float lens = pow(saturate(1.0 - abs(dist - holeR * 1.15) * 22.0), 2.0) * uProgress;

    float hole = 1.0 - smoothstep(holeR * 0.85, holeR, dist);

    float sector = floor((angle / 6.2831853 + 0.5) * 32.0);
    float streak = pow(hash21(float2(sector, uSeed)), 2.0) * saturate(1.0 - dist * 1.1) * smoothstep(holeR, holeR + 0.1, dist) * (0.5 + 0.5 * sin(t * 8.0 - dist * 25.0 + sector));
    streak *= uProgress;

    float wave = saturate(1.0 - abs(dist - uDetonate * 1.1) * 6.0) * (1.0 - uDetonate) * step(0.001, uDetonate);
    float boom = pow(saturate(1.0 - dist * (1.0 + uDetonate * 3.0)), 1.5) * saturate(uDetonate * 4.0) * (1.0 - uDetonate);

    float3 violet = float3(0.55, 0.20, 1.00);
    float3 azure  = float3(0.30, 0.65, 1.00);
    float3 white  = float3(1.00, 0.99, 1.00);

    float3 finalColor = lerp(violet, azure, plasma) * disk * 1.5;
    finalColor += white * lens * 2.0;
    finalColor += violet * streak * 1.2;
    finalColor += lerp(azure, white, 0.5) * wave * 1.8;
    finalColor = lerp(finalColor, white, saturate(boom));

    float alpha = saturate(disk + lens + streak + wave + boom);

    finalColor *= (1.0 - hole);
    alpha *= (1.0 - hole * 0.5);

    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass SpaceRiftPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
