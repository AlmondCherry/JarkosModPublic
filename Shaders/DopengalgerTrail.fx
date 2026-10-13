sampler uImage0 : register(s0);

float uTime;
float uOpacity;
float3 uColorHot;
float3 uColorMid;
float3 uColorCold;
float4x4 uWorldViewProjection;


struct VSInput  { float4 Pos : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
struct VSOutput { float4 Pos : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };

VSOutput VertexFunction(VSInput input)
{
    VSOutput output;
    output.Pos = mul(input.Pos, uWorldViewProjection);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}


float hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float vnoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + float2(1, 0));
    float c = hash21(i + float2(0, 1));
    float d = hash21(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}


float fbm(float2 p)
{
    float v = 0.0;
    float amp = 0.55;
    for (int k = 0; k < 4; k++)
    {
        v += vnoise(p) * amp;
        p = p * 2.15 + 17.0;
        amp *= 0.5;
    }
    return v;
}

float4 PixelFunction(VSOutput input) : COLOR0
{
    float2 uv = input.UV; 

    
    float fade = pow(1.0 - uv.x, 1.4);
    float mask = smoothstep(0.0, 0.12, uv.y) * smoothstep(1.0, 0.98, uv.y + 0.0001);

    
    float neb1 = fbm(float2(uv.x * 5.0 + uTime * 1.2,  uv.y * 3.0 - uTime * 0.4));
    float neb2 = fbm(float2(uv.x * 9.0 - uTime * 2.0,  uv.y * 5.0 + uTime * 0.7));
    float nebula = neb1 * 0.65 + neb2 * 0.35;

    
    float flow = sin((uv.x * 22.0 - uTime * 9.0) + nebula * 6.0) * 0.5 + 0.5;
    flow = pow(flow, 3.0) * fade;

    
    float3 color = lerp(uColorCold, uColorMid, saturate(nebula * 1.4));
    color = lerp(color, uColorHot, saturate(flow + uv.y * uv.y * fade * 0.6));

    
    float2 sp = float2(uv.x * 26.0, uv.y * 7.0);
    float2 cell = floor(sp);
    float2 cuv = frac(sp) - 0.5;
    float rnd = hash21(cell);
    float twinkle = sin(uTime * 8.0 + rnd * 40.0) * 0.5 + 0.5;
    float star = step(0.92, rnd) * twinkle;
    float dist = length(cuv);
    float core = star * smoothstep(0.25, 0.0, dist);
    float flare = star * smoothstep(0.4, 0.0, abs(cuv.x)) * smoothstep(0.08, 0.0, abs(cuv.y));
    flare += star * smoothstep(0.4, 0.0, abs(cuv.y)) * smoothstep(0.08, 0.0, abs(cuv.x));
    color += float3(0.75, 0.82, 1.0) * (core * 2.0 + flare) * fade;

    
    float edge = smoothstep(0.78, 0.98, uv.y) * fade;
    color += float3(0.45, 0.70, 1.0) * edge * (1.1 + flow);

    
    float head = smoothstep(0.16, 0.0, uv.x);
    color += float3(0.9, 0.85, 1.0) * head * (0.6 + nebula * 0.5);

    float alpha = mask * fade * (0.30 + nebula * 0.45 + flow * 0.4 + edge * 0.5);
    alpha *= uOpacity * input.Color.a;

    return float4(color * alpha, alpha);
}

technique Technique1
{
    pass TrailPass
    {
        VertexShader = compile vs_3_0 VertexFunction();
        PixelShader  = compile ps_3_0 PixelFunction();
    }
}
