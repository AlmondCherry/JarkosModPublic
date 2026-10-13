sampler uImage0 : register(s0);

float uTime;
float uSeed;
float uFade;
float uPulse;
float uMaster;

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

    float swirl = angle + 4.5 / (dist + 0.25) + t * 1.4;
    float2 swirlUV = float2(cos(swirl), sin(swirl)) * dist * 3.5;
    float plasma = fbm(swirlUV + uSeed + t * 0.5);
    float vortexMask = smoothstep(0.10, 0.28, dist) * (1.0 - smoothstep(0.50, 0.80, dist));
    float vortex = vortexMask * (0.18 + plasma * 0.5);

    float innerR = 0.30;
    float sector = floor((angle / 6.2831853 + 0.5) * 24.0);
    float rune = 0.6 + 0.4 * sin(t * 3.0 + hash21(float2(sector, uSeed)) * 6.2831853);
    float ring1 = pow(saturate(1.0 - abs(dist - innerR) * 16.0), 2.0) * rune;

    float wobble = sin(angle * 6.0 - t * 2.2) * 0.02;
    float outerR = 0.52 + wobble;
    float ring2 = pow(saturate(1.0 - abs(dist - outerR) * 24.0), 2.0) * (0.7 + 0.3 * sin(angle * 12.0 + t * 4.0));

    float strSector = floor((angle / 6.2831853 + 0.5) * 40.0);
    float streak = pow(hash21(float2(strSector, uSeed + 3.7)), 2.5) * saturate(1.0 - dist * 1.3) * smoothstep(0.12, 0.22, dist) * (0.5 + 0.5 * sin(t * 6.0 + dist * 20.0 + strSector));

    float waveR = 0.30 + uPulse * 0.75;
    float wave = pow(saturate(1.0 - abs(dist - waveR) * 14.0), 2.0) * (1.0 - uPulse) * (1.0 - uPulse);

    float coreGlow = pow(saturate(1.0 - dist * 3.2), 2.0);
    float edgeFade = 1.0 - smoothstep(0.80, 1.0, dist);

    float3 violet  = float3(0.55, 0.22, 1.00);
    float3 magenta = float3(0.90, 0.15, 0.62);
    float3 azure   = float3(0.32, 0.65, 1.00);
    float3 white   = float3(1.00, 0.97, 1.00);

    float3 finalColor = lerp(violet, magenta, plasma) * vortex * 0.7;
    finalColor += lerp(magenta, white, rune * 0.5) * ring1 * 0.9;
    finalColor += azure * ring2 * 0.65;
    finalColor += violet * streak * 0.55;
    finalColor += lerp(azure, white, 0.5) * wave * 1.0;
    finalColor += magenta * coreGlow * 0.45;

    float smoothFade = uFade * uFade * saturate(uMaster);
    float alpha = saturate(vortex * 0.6 + ring1 * 0.7 + ring2 * 0.55 + streak * 0.45 + wave * 0.8 + coreGlow * 0.35) * edgeFade * smoothFade;

    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass Phase3AuraPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
