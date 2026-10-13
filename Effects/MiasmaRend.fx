sampler uImage0 : register(s0);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uProgress;
float uTime;
float uIntensity;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);

    float shardT = uProgress - 0.9;
    shardT = shardT * 10.0;
    shardT = saturate(shardT);
    float drop = shardT * shardT;
    drop = drop * 0.9;

    float2 pc;
    pc.x = coords.x;
    pc.y = coords.y - drop;

    float fade = 1.0 - shardT;
    fade = saturate(fade);

    float t1 = uProgress - 0.05;
    t1 = t1 * 8.0;
    t1 = saturate(t1);
    float d1 = (pc.x - 0.20) * 0.8944 + (pc.y - 0.15) * 0.4472;
    float ad1 = abs(d1);
    float w1 = t1 * 0.05;
    float core1 = (w1 - ad1) * 60.0;
    core1 = saturate(core1);
    float e1 = ad1 - w1;
    e1 = abs(e1);
    float edge1 = 1.0 - e1 * 140.0;
    edge1 = saturate(edge1);
    edge1 = edge1 * t1;

    float t2 = uProgress - 0.30;
    t2 = t2 * 8.0;
    t2 = saturate(t2);
    float d2 = (pc.x - 0.85) * -0.4472 + (pc.y - 0.30) * 0.8944;
    float ad2 = abs(d2);
    float w2 = t2 * 0.05;
    float core2 = (w2 - ad2) * 60.0;
    core2 = saturate(core2);
    float e2 = ad2 - w2;
    e2 = abs(e2);
    float edge2 = 1.0 - e2 * 140.0;
    edge2 = saturate(edge2);
    edge2 = edge2 * t2;

    float t3 = uProgress - 0.55;
    t3 = t3 * 8.0;
    t3 = saturate(t3);
    float d3 = (pc.x - 0.50) * 0.9701 + (pc.y - 0.80) * -0.2425;
    float ad3 = abs(d3);
    float w3 = t3 * 0.05;
    float core3 = (w3 - ad3) * 60.0;
    core3 = saturate(core3);
    float e3 = ad3 - w3;
    e3 = abs(e3);
    float edge3 = 1.0 - e3 * 140.0;
    edge3 = saturate(edge3);
    edge3 = edge3 * t3;

    float coreTotal = core1 + core2 + core3;
    coreTotal = saturate(coreTotal);
    float edgeTotal = edge1 + edge2 + edge3;
    edgeTotal = saturate(edgeTotal);

    float neb = sin(pc.x * 9.0 + uTime * 0.5);
    float nebB = sin(pc.y * 11.0 - uTime * 0.4);
    neb = neb * nebB;
    neb = 0.5 + neb * 0.5;

    float st = sin(pc.x * 143.0 + pc.y * 97.0);
    float stB = sin(pc.x * 61.0 - pc.y * 83.0 + uTime * 2.0);
    st = st * stB;
    st = saturate(st);
    st = st * st;
    st = st * st;
    st = st * st;

    float3 cosmic;
    cosmic.r = 0.05 + 0.40 * neb + st;
    cosmic.g = 0.01 + 0.10 * neb + st;
    cosmic.b = 0.14 + 0.75 * neb + st;

    float glowPhase = uProgress - 0.75;
    glowPhase = glowPhase * 8.0;
    glowPhase = saturate(glowPhase);
    float glowAmp = 0.6 + glowPhase * 1.6;

    float sparkle = sin(pc.x * 90.0 + pc.y * 70.0 + uTime * 8.0);
    sparkle = 0.5 + sparkle * 0.5;
    sparkle = 0.4 + sparkle * 0.6;

    float whiteAmt = edgeTotal * glowAmp;
    whiteAmt = whiteAmt * sparkle;
    whiteAmt = whiteAmt * fade;

    float coreMix = coreTotal * fade;
    float3 col = lerp(baseColor.rgb, cosmic, coreMix);
    col.r = col.r + whiteAmt;
    col.g = col.g + whiteAmt;
    col.b = col.b + whiteAmt;

    col.r = col.r + uIntensity * 0.9;
    col.g = col.g + uIntensity * 0.85;
    col.b = col.b + uIntensity;

    float4 result;
    result.rgb = col;
    result.a = baseColor.a;

    return result * sampleColor;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}