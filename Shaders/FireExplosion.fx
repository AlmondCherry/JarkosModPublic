



sampler noiseTex : register(s0);

matrix uWorldViewProjection;
float uTime;
float uProgress;
float4 uColorHot;
float4 uColorMid;
float4 uColorEdge;

struct VSInput  { float2 Pos : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
struct VSOutput { float4 Pos : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };

VSOutput Vert(VSInput input)
{
    VSOutput o;
    o.Pos = mul(float4(input.Pos, 0, 1), uWorldViewProjection);
    o.Color = input.Color;
    o.UV = input.UV;
    return o;
}

float4 Pixel(VSOutput input) : COLOR0
{
    float2 uv = input.UV - 0.5;          
    float dist = length(uv) * 2.0;       
    float angle = atan2(uv.y, uv.x);

    
    float n1 = tex2D(noiseTex, float2(angle * 0.6 + uTime * 0.5, dist * 1.5 - uTime * 2.0)).r;
    float n2 = tex2D(noiseTex, float2(angle * 1.4 - uTime * 0.8, dist * 3.0 + 0.37)).r;
    float noise = n1 * 0.65 + n2 * 0.35;

    
    float grow = 1.0 - pow(1.0 - saturate(uProgress * 1.25), 3.0);  
    float radius = grow * (0.75 + noise * 0.35);                     
    float ball = smoothstep(radius, radius - 0.35, dist);

    
    float burnout = smoothstep(uProgress * 1.3 - 0.45, uProgress * 1.3, dist + noise * 0.25);
    ball *= burnout;

    
    float fade = 1.0 - smoothstep(0.55, 1.0, uProgress);
    ball *= fade;

    
    float heat = saturate(ball * 1.8 - dist * 0.6 + noise * 0.3);
    float4 col = lerp(uColorEdge, uColorMid, saturate(heat * 1.4));
    col = lerp(col, uColorHot, saturate(heat * heat * 1.9));

    
    float waveR = saturate(uProgress * 1.6) * 0.98;
    float ring = smoothstep(0.05, 0.0, abs(dist - waveR)) * (1.0 - uProgress);
    col += uColorHot * ring * 1.5;

    col.a = 1.0;
    return col * (ball + ring) * input.Color;
}

technique Technique1
{
    pass ExplosionPass
    {
        VertexShader = compile vs_3_0 Vert();
        PixelShader = compile ps_3_0 Pixel();
    }
}
