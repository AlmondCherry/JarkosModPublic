matrix uWorldViewProjection;
float uTime;
float3 uColor;
float3 uSecondaryColor;

struct VSIn  { float2 pos : POSITION0; float4 col : COLOR0; float2 uv : TEXCOORD0; };
struct VSOut { float4 pos : SV_POSITION; float4 col : COLOR0; float2 uv : TEXCOORD0; };

VSOut Vert(VSIn input) {
    VSOut o;
    o.pos = mul(float4(input.pos, 0, 1), uWorldViewProjection);
    o.col = input.col;
    o.uv = input.uv;
    return o;
}

float4 Frag(VSOut input) : COLOR0 {
    
    float edge = 1.0 - input.uv.y;
    float core = pow(edge, 3.0);
    float fade = input.uv.x * input.uv.x;                       
    float scan = 0.85 + 0.15 * sin(input.uv.x * 40.0 - uTime * 30.0); 
    float3 col = lerp(uColor, uSecondaryColor, core) * scan;
    return float4(col * (edge * fade), 0); 
}

technique Technique1 {
    pass TrailPass {
        VertexShader = compile vs_2_0 Vert();
        PixelShader  = compile ps_2_0 Frag();
    }
}