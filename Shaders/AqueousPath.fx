


float4x4 uTransform;
float uTime;
float uGlobalPhase; 

struct VSInput
{
    float2 Pos   : POSITION0;
    float4 Color : COLOR0;
    float2 UV    : TEXCOORD0;
};

struct VSOutput
{
    float4 Pos   : POSITION0;
    float4 Color : COLOR0;
    float2 UV    : TEXCOORD0;
};

VSOutput Vert(VSInput input)
{
    VSOutput o;
    o.Pos = mul(float4(input.Pos, 0.0, 1.0), uTransform);
    o.Color = input.Color;
    o.UV = input.UV;
    return o;
}

float hash(float2 p)
{
    p = frac(p * float2(127.1, 311.7));
    p += dot(p, p + 34.53);
    return frac(p.x * p.y);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1, 0));
    float c = hash(i + float2(0, 1));
    float d = hash(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float fbm(float2 p)
{
    float v = 0.0;
    float amp = 0.55;
    for (int i = 0; i < 3; i++)
    {
        v += amp * noise(p);
        p = p * 2.07 + 17.3;
        amp *= 0.5;
    }
    return v;
}

float4 Frag(VSOutput input) : COLOR0
{
    float2 uv   = input.UV;        
    float fade  = input.Color.r;   
    float phase = input.Color.g;   
    float head  = input.Color.b;   
    float alpha = input.Color.a;   

    float flowT = uTime * 3.0;

    
    float wobble =
        sin(uv.x * 34.0 - flowT * 4.0) * 0.10 +
        sin(uv.x * 13.0 + flowT * 2.3) * 0.16 +
        (fbm(float2(uv.x * 9.0 - flowT, flowT * 0.5)) - 0.5) * 0.35;
    float v = uv.y + wobble * (0.35 + 0.65 * head); 

    float centered = abs(v - 0.5) * 2.0; 

    
    float edgeNoise = fbm(float2(uv.x * 22.0 - flowT * 2.0, v * 6.0 + flowT));
    float edge = 0.92 - 0.28 * edgeNoise;
    float body = smoothstep(edge, edge - 0.25, centered);

    
    float n1 = fbm(float2(uv.x * 16.0 - flowT * 2.6, v * 4.0));
    float n2 = fbm(float2(uv.x * 31.0 - flowT * 4.9, v * 8.0 + 5.0));
    float water = n1 * 0.65 + n2 * 0.35;

    
    float caustic = pow(saturate(1.0 - abs(water - 0.55) * 3.2), 5.0);
    float foam = smoothstep(0.55, 0.95, centered) * smoothstep(0.45, 0.8, n2);

    
    float ph = saturate(phase + uGlobalPhase * 0.25);
    float3 cDeep   = lerp(float3(0.02, 0.18, 0.45), float3(0.16, 0.02, 0.30), ph);
    float3 cMid    = lerp(float3(0.05, 0.45, 0.85), float3(0.45, 0.10, 0.75), ph);
    float3 cBright = lerp(float3(0.45, 0.85, 1.00), float3(0.85, 0.55, 1.00), ph);

    float3 col = lerp(cDeep, cMid, water);
    col += cBright * caustic * (0.9 + head * 1.2);
    col += cBright * foam * 0.8;

    
    float spark = pow(saturate(fbm(float2(uv.x * 44.0 + flowT * 6.0, v * 12.0)) - 0.45) * 2.6, 2.0);
    col += float3(0.9, 0.5, 1.2) * spark * ph * 1.6;

    
    col += cBright * head * 0.9;
    col *= 0.85 + 0.45 * fade;

    float a = saturate(body * fade * alpha * 1.4); 
    return float4(col * a, a); 
}

technique AqueousPathTech
{
    pass AqueousPass
    {
        VertexShader = compile vs_3_0 Vert();
        PixelShader  = compile ps_3_0 Frag();
    }
}
