
sampler uImage0 : register(s0);
sampler uImage1 : register(s1); 

float3 uColor;
float3 uSecondaryColor;
float  uOpacity;
float  uSaturation; 
float  uRotation;
float  uTime;
float4 uSourceRect;
float2 uWorldPosition;
float  uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
matrix uWorldViewProjection;
float4 uShaderSpecificData;

struct VSInput  { float4 Position : POSITION0; float4 Color : COLOR0; float2 TexCoord : TEXCOORD0; };
struct VSOutput { float4 Position : SV_POSITION; float4 Color : COLOR0; float2 TexCoord : TEXCOORD0; };

VSOutput MainVS(in VSInput input)
{
    VSOutput o = (VSOutput)0;
    o.Position = mul(input.Position, uWorldViewProjection);
    o.Color    = input.Color;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 MainPS(VSOutput input) : COLOR0
{
    
    float2 uv   = input.TexCoord;
    float  time = uTime;
    float  dir  = (uSaturation >= 0.0) ? 1.0 : -1.0;

    float n1 = tex2D(uImage1, float2(uv.x * 1.5 - time * 0.9 * dir, uv.y * 0.8 + time * 0.10)).r;
    float n2 = tex2D(uImage1, float2(uv.x * 3.1 + time * 0.4 * dir, uv.y * 1.7 - time * 0.60)).r;

    float2 fuv = uv;
    fuv.y += (n1 - 0.5) * 0.35 * uv.x;
    fuv.x += (n2 - 0.5) * 0.15;

    
    float c1 = tex2D(uImage1, fuv * float2(2.2, 1.2) + float2(-time * 1.4 * dir,  time * 0.20)).r;
    float c2 = tex2D(uImage1, fuv * float2(3.4, 2.0) + float2( time * 1.1 * dir, -time * 0.35)).r;
    float caustic = pow(saturate(c1 * c2 * 2.4), 3.0);

    
    float crest     = pow(saturate(1.0 - fuv.y * 2.2), 3.0);
    float foamNoise = tex2D(uImage1, float2(fuv.x * 5.0 - time * 2.2 * dir, fuv.y * 3.0)).r;
    float foam      = crest * smoothstep(0.35, 0.75, foamNoise) * 2.0;

    
    float depth = saturate(fuv.y);
    float3 col  = lerp(uColor, uSecondaryColor, depth * depth);

    float bands = sin(fuv.x * 22.0 - time * 9.0 * dir + n1 * 7.0) * 0.5 + 0.5;
    col *= lerp(0.72, 1.15, bands);

    col += caustic * float3(0.45, 0.85, 1.00) * (1.0 - depth * 0.6);
    col += foam    * float3(0.90, 1.00, 1.00);
    col += pow(saturate(c2), 9.0) * 2.5 * float3(0.7, 0.95, 1.0);
    col += pow(saturate(1.0 - uv.x * 3.5), 3.0) * float3(0.5, 0.85, 1.0);

    
    
    float body = smoothstep(0.0, 0.15, uv.y) * smoothstep(1.0, 0.80, uv.y);
    float tail = pow(saturate(1.0 - uv.x), 0.8);

    float dissolve = smoothstep(uv.x - 0.2, uv.x + 0.3, n2 * 0.6 + n1 * 0.4 + 0.35);
    dissolve = lerp(0.35, 1.0, dissolve); 

    float alpha = saturate(body * (0.65 + caustic * 0.45) + foam * 0.6)
                * tail * dissolve * uOpacity;

    return float4(col, 1.0) * alpha * input.Color;
}

technique Technique1
{
    pass AqueousTrailPass
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader  = compile ps_3_0 MainPS();
    }
}
