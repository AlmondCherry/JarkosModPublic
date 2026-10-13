sampler uImage0 : register(s0);

float uTime;

float2 hash22(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(dot(hash22(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)), dot(hash22(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x), 
    lerp(dot(hash22(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)), dot(hash22(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}

float fbm(float2 p)
{
    float f = 0.0;
    f += 0.5000 * noise(p); p = p * 2.02;
    f += 0.2500 * noise(p); p = p * 2.03;
    f += 0.1250 * noise(p); 
    return f;
}

float fbmJagged(float2 p)
{
    float f = 0.0;
    f += 0.5000 * abs(noise(p) * 2.0 - 1.0); p = p * 2.02;
    f += 0.2500 * abs(noise(p) * 2.0 - 1.0);
    return f;
}

float getLightning(float2 uv, float seed, float time)
{
    float angle = seed * 12.345 + time * 0.1;
    float2 dir = float2(cos(angle), sin(angle));
    float2 perp = float2(-dir.y, dir.x);
    
    float dist = dot(uv, perp);
    float proj = dot(uv, dir);
    
    float jitter = fbmJagged(float2(proj * 2.0, time * 12.0 + seed)) * 1.5;
    jitter += fbmJagged(float2(proj * 5.0, time * 20.0 - seed)) * 0.5;
    
    float d = abs(dist + (jitter - 0.7) * 0.6);

    float thickness = 0.015 + 0.015 * fbm(float2(time * 30.0, seed));
    
    if (d < thickness) {
        float glow = 1.0 - (d / thickness);
        
        float flash = fbm(float2(time * 15.0, seed * 7.0));
        glow *= step(0.5, flash); 
        
        return pow(glow, 1.5);
    }
    return 0.0;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 pos = coords * 2.0 - 1.0;
    float dist = length(pos);
    
    if (dist > 1.0)
        return float4(0, 0, 0, 0);

    float edgeFade = smoothstep(1.0, 0.85, dist);

    float2 uv = pos * 2.5;
    
    float2 bgUv = uv;
    bgUv.y += uTime * 0.05; 
    float bgNoise = fbm(bgUv + float2(uTime * 0.02, uTime * 0.03));
    bgNoise = bgNoise * 0.5 + 0.5;
    
    float3 cloudDark = float3(0.02, 0.03, 0.05);
    float3 cloudLight = float3(0.08, 0.12, 0.18);
    float3 cloudBase = lerp(cloudDark, cloudLight, pow(bgNoise, 1.5));
    
    float flashTrigger = fbm(float2(uTime * 0.8, fbm(uv * 0.5)));
    float flashIntensity = smoothstep(0.85, 1.0, flashTrigger);
    
    float flashPosNoise = fbm(uv * 1.5 - uTime * 0.1);
    flashIntensity *= smoothstep(0.0, 1.0, flashPosNoise + 0.5);
    
    float3 flashColor = float3(1.0, 0.9, 0.4) * flashIntensity * 2.5;
    float3 bgColor = cloudBase + flashColor;
    
    float lightningSum = 0.0;
    lightningSum = max(lightningSum, getLightning(uv, 1.1, uTime));
    lightningSum = max(lightningSum, getLightning(uv, 5.5, uTime));
    
    lightningSum *= 0.5;
    
    float3 lightningCore = float3(1.0, 1.0, 1.0);
    float3 lightningGlow = float3(0.1, 0.7, 1.0);
    float3 lightningColorFinal = lerp(lightningGlow, lightningCore, pow(lightningSum * 2.0, 2.5)) * lightningSum * 2.5;
    
    float3 finalColor = bgColor + lightningColorFinal;
    float alpha = edgeFade;
    
    return float4(finalColor * alpha, alpha);
}

technique Technique1
{
    pass ThunderCloudPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
