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
    for (int i = 0; i < 4; ++i) {
        value += amplitude * noise(st);
        st *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 p = (coords - 0.5) * 2.0; 
    float r = length(p);
    
    
    float mask = smoothstep(1.0, 0.98, r);
    
    
    float angle = atan2(p.y, p.x);
    float edgeDistortion = fbm(float2(cos(angle) * 3.0, sin(angle) * 3.0) + uTime * 2.0) * 0.15;
    
    float deformedR = r - edgeDistortion;
    float deformedMask = 1.0 - smoothstep(0.85, 0.90, deformedR); 
    
    
    float whiteOutline = smoothstep(0.80, 0.85, deformedR) * (1.0 - smoothstep(0.85, 0.95, deformedR));
    
    
    float2 st = coords * 5.0;
    float inkNoise = fbm(st + float2(uTime * 1.5, uTime * 1.0));
    float inkNoise2 = fbm(st * 2.0 - float2(uTime * 0.5, uTime * 2.0));
    
    float inkIntensity = (inkNoise * 0.6 + inkNoise2 * 0.4);
    
    
    float3 inkColor = lerp(float3(0.0, 0.0, 0.0), float3(0.15, 0.15, 0.15), inkIntensity);
    
    
    float3 finalColor = lerp(inkColor, float3(1.0, 1.0, 1.0), whiteOutline);
    
    float alpha = deformedMask;
    
    float4 baseColor = tex2D(uImage0, coords);
    return float4(finalColor * alpha, alpha) * baseColor.a;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
