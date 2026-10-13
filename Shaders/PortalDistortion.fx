
















sampler uImage0 : register(s0);

float3 uColor;          
float3 uSecondaryColor; 
float uOpacity;         
float uProgress;        
float uIntensity;       
float2 uImageOffset;    
float2 uScreenPosition; 
float2 uTargetPosition; 

float Saturate01(float v)
{
    return min(1.0, max(0.0, v));
}

float SmoothStep01(float v)
{
    v = Saturate01(v);
    return v * v * (3.0 - 2.0 * v);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float aspect = screenSize.x / screenSize.y;

    
    
    
    float2 center = (uTargetPosition - uScreenPosition) / screenSize;

    
    float2 p = coords - center;
    p.x *= aspect;

    float dist = length(p);
    float angle = atan2(p.y, p.x);

    
    
    
    float swirlFalloff = exp(-dist * 9.0);
    float swirl = swirlFalloff * (0.45 + uIntensity * 0.6);

    
    float pullFalloff = exp(-dist * 7.0);
    float radialPull = pullFalloff * (0.012 + uIntensity * 0.060);

    
    float newAngle = angle + swirl;
    float newDist = max(dist - radialPull, 0.0);

    float2 warped;
    warped.x = cos(newAngle) * newDist;
    warped.y = sin(newAngle) * newDist;
    warped.x /= aspect;
    float2 warpedCoords = warped + center;

    
    
    
    
    float aberrationBand = 1.0 - Saturate01(abs(dist - 0.05) / 0.03);
    aberrationBand *= 0.005 * (0.5 + uIntensity * 0.7);

    float2 aberrationDir = float2(0.0, 0.0);
    if (dist > 0.0001)
        aberrationDir = p / dist;
    aberrationDir.x /= aspect;

    float3 sceneColor;
    sceneColor.r = tex2D(uImage0, warpedCoords + aberrationDir * aberrationBand).r;
    sceneColor.g = tex2D(uImage0, warpedCoords).g;
    sceneColor.b = tex2D(uImage0, warpedCoords - aberrationDir * aberrationBand).b;

    
    
    
    float ring = 1.0 - Saturate01(abs(dist - 0.045) / 0.018);
    ring = ring * ring * 0.55; 

    float halo = exp(-dist * 11.0) * 0.18; 

    float3 add = uColor * halo + uSecondaryColor * ring * 0.30;
    add *= uIntensity;

    
    
    
    
    float collapseT = Saturate01((uProgress - 0.5) / 0.5);
    float flash = exp(-dist * (12.0 - collapseT * 5.0)) * collapseT * 0.30;
    add += float3(1.0, 1.0, 1.0) * flash;

    float3 finalColor = sceneColor + add * uOpacity;
    return float4(finalColor, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
