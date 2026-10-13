sampler uImage0 : register(s0);

float2 uScreenResolution;
float2 uSkyOffset;
float uTime;
float uOpacity;
float uIntensity;
float uRadius;

float2 uDarknessCenter;
float uDarknessRadius;


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

    return lerp(a, b, u.x) +
            (c - a) * u.y * (1.0 - u.x) +
            (d - b) * u.x * u.y;
}


float fbm(float2 st) {
    float value = 0.0;
    float amplitude = 0.5;
    float2 shift = float2(100.0, 100.0);
    
    float2x2 rot = float2x2(cos(0.5), sin(0.5),
                            -sin(0.5), cos(0.5));
    for (int i = 0; i < 5; ++i) {
        value += amplitude * noise(st);
        st = mul(rot, st) * 2.0 + shift;
        amplitude *= 0.5;
    }
    return value;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float aspect = uScreenResolution.x / (uScreenResolution.y + 1.0);
    
    float centerX = (coords.x - 0.5) * aspect;
    float centerY = coords.y - 0.5;
    
    float distFromCenter = sqrt(centerX * centerX + centerY * centerY);
    
    
    float mask = saturate((uRadius - distFromCenter) / 0.05); 

    
    float2 absoluteWorldPos = coords * uScreenResolution + uSkyOffset;
    
    
    float2 st = absoluteWorldPos * 0.0015;

    
    float2 q = float2(0.0, 0.0);
    q.x = fbm(st + 0.02 * uTime);
    q.y = fbm(st + float2(1.0, 1.0));

    float2 r = float2(0.0, 0.0);
    r.x = fbm(st + 1.0 * q + float2(1.7, 9.2) + 0.15 * uTime);
    r.y = fbm(st + 1.0 * q + float2(8.3, 2.8) + 0.126 * uTime);

    float f = fbm(st + r);

    
    
    float colorValue = f * f * f * f * 1.5; 
    
    colorValue = saturate(colorValue);
    
    
    float3 col = float3(colorValue, colorValue, colorValue) * 0.15;
    
    
    float lines = sin(r.x * 12.0 + uTime * 2.0) * 0.5 + 0.5;
    lines = pow(lines, 15.0); 
    col += float3(lines, lines, lines) * 0.03; 

    
    col = max(col, float3(0.0, 0.0, 0.0));

    float distToDarkness = length(absoluteWorldPos - uDarknessCenter);
    float darknessMask = step(uDarknessRadius, distToDarkness); 
    col *= darknessMask;

    
    float alpha = uOpacity * mask;
    
    return float4(col * alpha, alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
