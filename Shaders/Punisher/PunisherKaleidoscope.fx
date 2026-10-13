sampler uImage0 : register(s0);

float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;
    
    
    float2 center = (uTargetPosition - uScreenPosition) / screenSize;
    
    float2 p = coords - center;
    p.x *= aspect; 
    
    float radius = max(length(p), 0.0001);
    float angle = atan2(p.y, p.x);
    
    
    float baseAngle = angle + sin(angle * 4.0) * 0.4 + sin(angle * 7.0) * 0.2;
    float noiseAngle = baseAngle + sin(radius * 60.0) * 0.05 + sin(radius * 200.0) * 0.01;
    float radial = abs(frac(noiseAngle * 1.909859) - 0.5); 
    
    
    float ringFreq = 20.0 + sin(angle * 8.0) * 10.0;
    float noiseRadius = radius + sin(angle * 30.0) * 0.02;
    float concentricDist = abs(frac(noiseRadius * ringFreq) - 0.5);
    
    float arcMask = step(0.4, sin(angle * 25.0 + radius * 100.0));
    float concentric = concentricDist + (1.0 - arcMask); 
    
    
    float crack = min(radial, concentric);
    
    
    float thickness = max(0.015 * uOpacity, 0.0001); 
    float crackIntensity = max(0.0, 1.0 - (crack / thickness));
    
    
    float spread = uOpacity * 1.5;
    float reveal = step(radius, spread + sin(angle * 15.0) * 0.1);
    crackIntensity *= reveal;
    crackIntensity *= step(0.001, uOpacity); 
    
    
    float2 refractDir = p / radius;
    float2 uv = coords + refractDir * 0.03 * crackIntensity * uOpacity;
    
    
    float aberration = 0.01 * crackIntensity * uOpacity;
    float2 uvR = uv + float2(aberration, 0.0);
    float2 uvB = uv - float2(aberration, 0.0);
    
    float r = tex2D(uImage0, uvR).r;
    float g = tex2D(uImage0, uv).g;
    float b = tex2D(uImage0, uvB).b;
    
    float4 color = float4(r, g, b, 1.0);
    
    
    float3 glow = float3(1.2, 0.1, 1.8) * crackIntensity * uOpacity * 2.0;
    float coreMask = step(crack, thickness * 0.3);
    glow += float3(1.0, 1.0, 1.0) * crackIntensity * uOpacity * 1.5 * coreMask;
    
    color.rgb += glow;
    
    
    color.rgb *= lerp(1.0, 0.5, uOpacity * radius);
    
    float4 originalColor = tex2D(uImage0, coords);
    return lerp(originalColor, color, step(0.001, uOpacity));
}

technique Technique1
{
    pass KaleidoscopePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
