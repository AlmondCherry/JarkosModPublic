sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float Smooth01(float value)
{
    value = Clamp01(value);
    return value * value * (3.0 - 2.0 * value);
}

float Hash21(float2 value)
{
    value = frac(value * float2(123.34, 456.21));
    value += dot(value, value + 45.32);
    return frac(value.x * value.y);
}

float StarTwinkle(float2 coords, float density, float cutoff, float time)
{
    float2 grid = coords * density;
    float2 cell = floor(grid);
    float2 local = frac(grid) - 0.5;
    float random = Hash21(cell);
    float spark = step(cutoff, random);
    float twinkle = 0.55 + 0.45 * sin(time * (2.4 + random * 4.6) + random * 31.0);
    float dist = length(local);
    float disk = pow(Clamp01(1.0 - dist / 0.18), 8.0);
    return spark * disk * twinkle;
}

float Wave(float dist, float radius, float thickness, float speed, float time)
{
    float wave = sin((dist - radius) * 26.0 - time * speed) * 0.5 + 0.5;
    return Clamp01(1.0 - abs(dist - radius) / thickness) * wave;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;

    
    float2 center = (uTargetPosition - uScreenPosition) / screenSize;

    float2 p = coords - center;
    p.x = p.x * aspect;
    float dist = length(p) + 0.0001;
    float2 radial = p / dist;
    float2 radialUv = float2(radial.x / aspect, radial.y);

    float progress = Clamp01(uProgress);
    float opacity = Clamp01(uOpacity);
    float power = Clamp01(uIntensity);

    float fadeIn = Smooth01(progress / 0.20);
    float fadeOut = 1.0 - Smooth01((progress - 0.42) / 0.58);
    float life = fadeIn * fadeOut * opacity;

    float time = progress * 6.2832;

    
    float wave1 = Wave(dist, 0.10 + progress * 1.55, 0.085 + power * 0.025, 11.0, time);
    float wave2 = Wave(dist, 0.04 + progress * 1.25, 0.060 + power * 0.018, 8.0, time + 1.7);
    float wave3 = Wave(dist, 0.20 + progress * 1.85, 0.110 + power * 0.030, 14.0, time + 3.3);
    float shock = Clamp01(wave1 * 0.85 + wave2 * 0.55 + wave3 * 0.45);

    
    
    
    
    
    
    float waveTail = 1.0 - Smooth01((progress - 0.55) / 0.45);
    float shockFade = life * waveTail;
    float shockColor = shock * shockFade;

    
    
    
    
    float warpStrength = (shock * 0.055 + Smooth01(progress / 0.16) * 0.035) * power * life * 1.5;
    float2 warpUv = coords + radialUv * warpStrength * (1.0 - Clamp01(dist - 0.85));

    
    
    
    float chroma = (0.0040 + power * 0.0035) * (life + shockColor * 0.6);
    float2 uvR = warpUv + radialUv * chroma;
    float2 uvG = warpUv;
    float2 uvB = warpUv - radialUv * chroma;

    float r = tex2D(uImage0, uvR).r;
    float g = tex2D(uImage0, uvG).g;
    float b = tex2D(uImage0, uvB).b;
    float3 sceneColor = float3(r, g, b);
    float3 baseColor = tex2D(uImage0, coords).rgb;

    float luminance = baseColor.r * 0.30 + baseColor.g * 0.59 + baseColor.b * 0.11;

    
    float vignette = Smooth01((dist - 0.10) / 1.30);
    float washAmount = (0.30 + power * 0.22 + vignette * (0.20 + power * 0.10)) * life;
    washAmount = Clamp01(washAmount);
    float3 cosmicWash = float3(
        luminance * 0.30 + uColor.r * 0.42,
        luminance * 0.10 + uColor.g * 0.18,
        luminance * 0.55 + uSecondaryColor.b * 0.32);

    
    float starTime = time * 0.55 + 12.7;
    float starsLayerA = StarTwinkle(coords * float2(aspect, 1.0), 92.0, 0.978, starTime);
    float starsLayerB = StarTwinkle(coords * float2(aspect, 1.0) + float2(0.31, 0.17), 168.0, 0.985, starTime + 4.1);
    float stars = (starsLayerA * 0.85 + starsLayerB * 0.55) * life * (0.45 + power * 0.45);

    
    float coronaProgress = Clamp01(progress / 0.30);
    float coronaFade = 1.0 - Clamp01((progress - 0.18) / 0.30);
    float corona = pow(Clamp01(1.0 - dist / (0.18 + power * 0.10)), 3.0) * coronaProgress * coronaFade * life;

    
    float halo = pow(Clamp01(1.0 - abs(dist - (0.10 + progress * 1.55)) / (0.32 + power * 0.10)), 2.0) * life * 0.6;

    float3 color = lerp(sceneColor, cosmicWash, washAmount);
    color = lerp(color, sceneColor, shockColor * 0.18); 
    color = color + uColor * corona * 0.95;
    color = color + uSecondaryColor * (halo * 0.45 + shockColor * 0.55) * (0.35 + power * 0.55);
    color = color + float3(0.92, 0.88, 1.0) * stars;
    color = color * (1.0 - vignette * (0.18 + power * 0.10) * life);

    return float4(Clamp01(color.r), Clamp01(color.g), Clamp01(color.b), 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
