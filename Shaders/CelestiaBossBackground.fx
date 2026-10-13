float2 uImageOffset;
float uTime;
float uOpacity;
float iTime;
float2 uResolution;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float SmoothStep(float mn, float mx, float x)
{
    float t = Clamp01((x - mn) / (mx - mn));
    return t * t * (3.0 - 2.0 * t);
}

float2 Rotate(float2 p, float a)
{
    float c = cos(a);
    float s = sin(a);
    return float2(c * p.x - s * p.y, s * p.x + c * p.y);
}

float2 Disp(float t)
{
    return float2(sin(t * 0.22), cos(t * 0.175)) * 2.0;
}

float3 M3(float3 p)
{
    return float3(
        dot(p, float3(0.6434234, 1.0814562, -1.3856681)),
        dot(p, float3(-1.6962191, 0.6301643, -0.2957339)),
        dot(p, float3(0.2926266, 1.3432028, 1.1838427))
    );
}

float2 Map(float3 p, float prm)
{
    float3 p2 = p;
    p2.xy -= Disp(p.z);
    p.xy = Rotate(p.xy, sin(p.z + iTime) * (0.1 + prm * 0.05) + iTime * 0.09);

    float cl = dot(p2.xy, p2.xy);
    float d = 0.0;
    p *= 0.61;
    float dspAmp = 0.1 + prm * 0.2;

    float3 pA = p + sin(p.zxy * 0.75 + iTime * 0.8) * dspAmp;
    d -= abs(dot(cos(pA), sin(pA.yzx)));

    float3 pB = M3(pA);
    pB += sin(pB.zxy * 1.05 + iTime * 1.12) * dspAmp;
    d -= abs(dot(cos(pB), sin(pB.yzx)) * 0.57);

    float3 pC = M3(pB);
    pC += sin(pC.zxy * 1.47 + iTime * 1.56) * dspAmp;
    d -= abs(dot(cos(pC), sin(pC.yzx)) * 0.3249);

    d = abs(d + prm * 3.0) + prm * 0.3 - 2.5;
    return float2(d + cl * 0.2 + 0.25, cl);
}

float4 Render(float3 ro, float3 rd, float time, float prm)
{
    float4 rez = float4(0.0, 0.0, 0.0, 0.0);
    float t = 1.5;
    float fogT = 0.0;

    [loop]
    for (int i = 0; i < 30; i++)
    {
        if (rez.a > 0.99)
            break;

        float3 pos = ro + t * rd;
        float2 mpv = Map(pos, prm);
        float den = Clamp01(mpv.x - 0.3) * 1.12;
        float dn = min(3.0, max(0.0, mpv.x + 2.0));

        float4 col = float4(0.0, 0.0, 0.0, 0.0);
        if (mpv.x > 0.6)
        {
            col = float4(sin(float3(5.0, 0.4, 0.2) + mpv.y * 0.1 + sin(pos.z * 0.4) * 0.5 + 1.8) * 0.5 + 0.5, 0.08);
            col *= den * den * den;
            col.rgb *= Clamp01((mpv.x - 4.0) / -6.5) * 2.3;

            float dif = Clamp01(den * 0.68 + sin(pos.z * 0.27 + mpv.y * 0.03 + iTime) * 0.18);
            col.rgb *= den * (float3(0.018, 0.070, 0.100) + 1.35 * float3(0.050, 0.095, 0.055) * dif);
        }

        float fogC = exp(t * 0.2 - 2.2);
        col += float4(0.06, 0.11, 0.11, 0.1) * Clamp01(fogC - fogT);
        fogT = fogC;
        rez = rez + col * (1.0 - rez.a);
        t += min(0.3, max(0.09, 0.5 - dn * dn * 0.05));
    }

    return float4(Clamp01(rez.r), Clamp01(rez.g), Clamp01(rez.b), Clamp01(rez.a));
}


float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(max(uImageOffset, uResolution), float2(1.0, 1.0));
    float2 q = coords;
    float2 p = (coords * screenSize - 0.5 * screenSize) / screenSize.y;

    float time = iTime * 3.0;
    float3 ro = float3(0.0, 0.0, time);
    ro += float3(sin(iTime) * 0.5, 0.0, 0.0);
    ro.xy += Disp(ro.z) * 0.85;

    float tgtDst = 3.5;
    float prm = SmoothStep(-0.4, 0.4, sin(iTime * 0.3));

    float3 target = normalize(ro - float3(Disp(time + tgtDst) * 0.85, time + tgtDst));
    float3 rightdir = normalize(cross(target, float3(0.0, 1.0, 0.0)));
    float3 updir = normalize(cross(rightdir, target));
    rightdir = normalize(cross(updir, target));
    float3 rd = normalize((p.x * rightdir + p.y * updir) - target);
    rd.xy = Rotate(rd.xy, -Disp(time + 3.5).x * 0.2);

    float4 scn = Render(ro, rd, time, prm);
    float3 col = scn.rgb;
    col = lerp(col.bgr, col.rgb, min(1.0, max(0.05, 1.0 - prm)));
    col = pow(max(col, float3(0.0001, 0.0001, 0.0001)), float3(0.55, 0.65, 0.6)) * float3(1.0, 0.97, 0.9);

    float vignette = Clamp01(16.0 * q.x * q.y * (1.0 - q.x) * (1.0 - q.y));
    col *= pow(max(vignette, 0.0001), 0.12) * 0.7 + 0.3;
    col = max(col, float3(0.01, 0.018, 0.020));

    
    float gray = dot(col, float3(0.299, 0.587, 0.114));
    col = lerp(float3(gray, gray, gray), col, 0.45);
    col *= 0.32;
    col = lerp(col, float3(0.0, 0.0, 0.0), 0.35);

    return float4(col, Clamp01(uOpacity));
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
