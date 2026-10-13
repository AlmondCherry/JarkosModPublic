sampler uImage0 : register(s0); 

float uTime;
float uOpacity; 
float2 uTargetPosition; 
float2 uScreenResolution;
float2 uScreenPosition;


float hash(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float noise(float2 x)
{
    float2 p = floor(x);
    float2 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    float n = p.x + p.y * 57.0;
    return lerp(lerp(hash(p + float2(0.0, 0.0)), hash(p + float2(1.0, 0.0)), f.x),
                lerp(hash(p + float2(0.0, 1.0)), hash(p + float2(1.0, 1.0)), f.x), f.y);
}

float fbm(float2 p)
{
    float f = 0.0;
    f += 0.5000 * noise(p); p = p * 2.02;
    f += 0.2500 * noise(p); p = p * 2.03;
    f += 0.1250 * noise(p); p = p * 2.01;
    f += 0.0625 * noise(p);
    return f / 0.9375;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 worldPos = uScreenPosition + coords * uScreenResolution;
    float2 diff = worldPos - uTargetPosition;
    float dist = length(diff);
    
    float radius = uOpacity;
    if (dist > radius || radius <= 0)
        return baseColor; 
        
    
    float alphaMask = 1.0 - smoothstep(radius - 150.0, radius, dist);
    
    
    float2 uv = worldPos * 0.002;
    float n1 = fbm(uv + float2(uTime * 0.1, uTime * 0.05));
    float n2 = fbm(uv * 2.0 - float2(uTime * 0.15, -uTime * 0.1));
    float n3 = fbm(uv * 4.0 + float2(-uTime * 0.05, uTime * 0.2));
    
    float cloud = n1 * 0.5 + n2 * 0.3 + n3 * 0.2;
    
    
    float3 cyan = float3(0.0, 0.8, 1.0);
    float3 darkCyan = float3(0.0, 0.2, 0.4);
    
    float3 cloudColor = lerp(darkCyan, cyan, cloud);
    
    
    float phase = frac(uTime * 0.285); 
    
    
    
    float envelope = 0.0;
    if (phase < 0.028) {
        envelope = phase / 0.028;
    } else if (phase < 0.313) {
        envelope = 1.0 - ((phase - 0.028) / 0.285);
    }
    
    
    envelope = envelope * envelope;
    
    
    float flicker = noise(float2(uTime * 30.0, 0.0)) * 0.5 + 0.5;
    
    float flash = envelope * flicker;
    float flashMask = smoothstep(0.3, 0.7, n1);
    cloudColor += float3(0.6, 0.9, 1.0) * flash * flashMask * 1.0;
    
    
    float rim = smoothstep(radius * 0.8, radius, dist) * (sin(uTime * 5.0) * 0.5 + 0.5);
    cloudColor += cyan * rim;
    
    
    float cloudAlpha = alphaMask * cloud * 0.8;
    float3 finalColor = lerp(baseColor.rgb, cloudColor, cloudAlpha);
    
    return float4(finalColor, baseColor.a);
}

technique Technique1
{
    pass Thundercloud
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
