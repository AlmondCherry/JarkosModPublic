sampler uImage0 : register(s0);

float uTime;
float uSeed;
float uOpacity;
float uCharge;

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
    float r = sqrt(dot(dir, dir)) * 2.0;
    float t = uTime;

    float sphereR = 0.60 + uCharge * 0.05;
    float rr = r / sphereR;
    float inside = 1.0 - smoothstep(0.965, 1.0, rr);

    float nz = sqrt(saturate(1.0 - rr * rr));
    float3 n = normalize(float3(dir.x * 2.0 / sphereR, dir.y * 2.0 / sphereR, max(nz, 0.001)));

    float3 L = normalize(float3(-0.45, -0.55, 0.72));
    float3 V = float3(0.0, 0.0, 1.0);
    float3 H = normalize(L + V);
    float diff = saturate(dot(n, L));
    float spec = pow(saturate(dot(n, H)), 48.0);
    float fres = pow(1.0 - saturate(nz), 2.2);
    float limb = 0.55 + 0.45 * nz;

    float2 pu = n.xy * (1.7 - nz * 0.7);
    float pl1 = fbm(pu * 3.0 + float2(t * 0.7, -t * 0.9) + uSeed);
    float pl2 = fbm(pu * 6.5 - float2(t * 1.3, t * 0.5) + uSeed * 2.7);
    float plasma = pl1 * 0.65 + pl2 * 0.35;

    float veins = pow(saturate(1.0 - abs(plasma - 0.5) * 6.0), 3.0);

    float core = pow(saturate(1.0 - rr * 1.2), 2.5) * (0.8 + 0.2 * sin(t * 4.0 + uSeed));

    float3 deep  = float3(0.03, 0.10, 0.45);
    float3 azure = float3(0.15, 0.48, 1.00);
    float3 hot   = float3(0.75, 0.92, 1.00);
    float3 white = float3(1.00, 1.00, 1.00);

    float3 body = lerp(deep, azure, saturate(diff * 0.85 + plasma * 0.5));
    body *= limb;
    body += hot * veins * (0.9 + uCharge * 0.7);
    body += white * spec * 1.3;
    body += azure * fres * 1.5;
    body = lerp(body, white, core * 0.9);

    float halo = pow(saturate(1.0 - r / (sphereR * 1.6)), 2.4) * (1.0 - inside);

    float3 finalColor = body * inside + azure * halo * (0.55 + uCharge * 0.35);
    float alpha = saturate(inside * (0.9 + fres * 0.3) + halo * 0.6) * uOpacity;

    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass BallLightningPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
