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
    
    float2 p = float2((coords.x - 0.5) * 2.0, 1.0 - coords.y);
    
    
    p.y -= 0.15;
    
    
    
    float d = abs(p.x) * 2.0; 
    float w = 0.8 - p.y * 1.2; 
    
    
    if (p.y < 0.0) {
        d = length(float2(p.x * 2.0, p.y * 5.0));
        w = 0.8;
    }
    
    
    float shape = (w - d) * 1.25;
    
    
    float2 st = float2(p.x * 1.5, p.y * 2.0 - uTime * 4.0);
    
    float n1 = fbm(st * 2.0);
    float n2 = fbm(st * 4.0 - float2(0.0, uTime * 2.0));
    float fireNoise = (n1 * 0.7 + n2 * 0.3);
    
    
    float turbulence = (fireNoise * 2.0 - 1.0) * max(0.0, p.y * 2.0);
    
    float flame = shape + turbulence;
    
    
    flame *= smoothstep(0.8, 0.4, p.y); 
    
    
    float3 darkGray = float3(0.1, 0.1, 0.15);
    float3 white = float3(1.0, 1.0, 1.0);
    
    
    float core = smoothstep(0.3, 0.7, flame);
    float outer = smoothstep(0.0, 0.3, flame);
    
    float3 color = lerp(darkGray, white, core);
    float alpha = saturate(outer);
    
    
    float4 baseColor = tex2D(uImage0, coords);
    
    return float4(color * alpha, alpha) * baseColor.a;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
