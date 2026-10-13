sampler uImage0 : register(s0);

texture uNoiseTexture;
sampler noiseSampler = sampler_state
{
    Texture = <uNoiseTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
};

float uTime;
float uProgress;

float dropShape(float2 p)
{
    float2 c = float2(0.0, 0.34);
    float r = 0.46;
    float dCircle = length(p - c) - r;

    float t = saturate((p.y + 0.85) / 1.19);
    float width = r * pow(t, 0.75);
    float dCone = abs(p.x) - width;

    float k = smoothstep(0.0, 0.34, p.y);
    return lerp(dCone, dCircle, k);
}

float4 PortalPS(float2 coords : TEXCOORD0) : COLOR0
{
    float2 p = coords * 2.0 - 1.0;

    float n1 = tex2D(noiseSampler, coords * float2(1.2, 0.8) + float2(0.0, -uTime * 0.40)).r;
    float n2 = tex2D(noiseSampler, coords * float2(2.6, 1.8) + float2(uTime * 0.10, -uTime * 0.75)).r;
    float n3 = tex2D(noiseSampler, coords * 0.9 + float2(uTime * 0.05, uTime * 0.08)).r;

    p.x += (n3 - 0.5) * 0.10 * uProgress;
    p.y += (n1 - 0.5) * 0.05;

    float d = dropShape(p);

    float body = smoothstep(0.015, -0.06, d);
    float rimCore = smoothstep(0.035, 0.0, abs(d));
    float rimGlow = smoothstep(0.13, 0.0, abs(d));
    float flicker = 0.75 + 0.5 * n2;

    float3 abyss = float3(0.02, 0.005, 0.06);

    float swirl = smoothstep(0.35, 0.95, n1 * 0.55 + n3 * 0.55);
    float3 col = abyss + float3(0.16, 0.04, 0.30) * swirl;

    float tongues = smoothstep(0.62, 0.85, n1 * 0.5 + n2 * 0.6);
    float nearEdge = smoothstep(-0.30, -0.02, d);
    float3 flameCol = lerp(float3(0.55, 0.15, 0.95), float3(1.0, 0.55, 0.12), saturate(-p.y * 0.8 + 0.2));
    col += flameCol * tongues * nearEdge * 1.3;

    float3 rimCol = lerp(float3(1.0, 0.5, 0.1), float3(0.25, 0.65, 1.0), saturate(p.y * 0.5 + 0.5));
    col += rimCol * nearEdge * 0.18;

    col *= body;

    col += rimCol * rimCore * 1.9 * flicker;
    col += rimCol * rimGlow * 0.55 * flicker;
    col += float3(1.0, 0.95, 0.8) * rimCore * rimCore * 0.8;

    float alpha = saturate(body * 0.93 + rimGlow * 0.6 * flicker) * uProgress;

    float mask = tex2D(uImage0, coords).a;
    alpha *= saturate(mask * 1.8);

    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass PortalPass
    {
        PixelShader = compile ps_3_0 PortalPS();
    }
}
