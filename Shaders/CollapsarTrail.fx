





sampler uImage0 : register(s0);

float uTime;
float3 uColor;           
float3 uSecondaryColor;  
float uOpacity;
float4x4 uWorldViewProjection;

struct VSInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}


float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
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

float4 PixelShaderFunction(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    
    

    
    
    float fade = pow(1.0 - uv.x, 1.4);
    
    float edge = smoothstep(0.0, 0.15, uv.y) * smoothstep(1.0, 0.45, uv.y);
    
    float rim = smoothstep(0.18, 0.0, uv.y) * (1.0 - uv.x);

    
    
    float n1 = noise(float2(uv.x * 10.0 - uTime * 8.0, uv.y * 4.0));
    float n2 = noise(float2(uv.x * 22.0 - uTime * 14.0, uv.y * 7.0 + 5.0));
    
    float bolts = smoothstep(0.62, 0.72, n1) + smoothstep(0.68, 0.76, n2) * 0.7;
    bolts *= (1.0 - uv.x); 

    
    float wave = 0.85 + 0.15 * sin(uv.x * 20.0 - uTime * 30.0);

    
    float3 col = lerp(uColor, uSecondaryColor, pow(uv.x, 0.8));
    
    col += float3(1, 1, 1) * bolts * 0.8;
    col = lerp(col, float3(1, 1, 1), rim * 0.85);

    float alpha = fade * edge * wave * uOpacity * input.Color.a;
    
    alpha += rim * 0.6 * uOpacity * input.Color.a;

    return float4(col, 1.0) * saturate(alpha);
}

technique Technique1
{
    pass TrailPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
