sampler uImage0 : register(s0);

float uTime;


float random(float2 st) {
    return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
}

float noise(float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);
    float a = random(i);
    float b = random(i + float2(1.0, 0.0));
    float c = random(i + float2(0.0, 1.0));
    float d = random(i + float2(1.0, 1.0));
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float fbm(float2 st) {
    float value = 0.0;
    float amplitude = 0.5;
    float2 shift = float2(100.0, 100.0);
    float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
    for (int i = 0; i < 4; ++i) {
        value += amplitude * noise(st);
        st = mul(rot, st) * 2.0 + shift;
        amplitude *= 0.5;
    }
    return value;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 uv = coords;
    
    
    float2 st = float2(uv.x * 10.0, uv.y * 2.0);
    
    
    st.x += uTime * 3.0;
    
    
    st.y += sin(st.x * 2.0 + uTime * 2.0) * 0.2;
    
    
    float n1 = fbm(st);
    float n2 = fbm(st * 2.0 - float2(uTime * 1.5, 0.0));
    
    float flow = (n1 * 0.7 + n2 * 0.3);
    
    
    flow = smoothstep(0.3, 0.7, flow);
    
    
    
    
    
    float3 darkRed = float3(0.4, 0.0, 0.05);
    float3 brightRed = float3(1.0, 0.1, 0.1);
    float3 white = float3(1.0, 0.9, 0.9);
    
    float3 color;
    if (flow < 0.5) {
        color = lerp(darkRed, brightRed, flow * 2.0);
    } else {
        color = lerp(brightRed, white, (flow - 0.5) * 2.0);
    }
    
    
    float edgeDarken = smoothstep(0.0, 0.2, uv.y) * smoothstep(1.0, 0.8, uv.y);
    color *= lerp(0.5, 1.0, edgeDarken);
    
    
    float4 baseColor = tex2D(uImage0, coords);
    
    return float4(color, 1.0) * baseColor.a;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
