




sampler uImage0 : register(s0);

float3 uColor;          
float3 uSecondaryColor; 
float uOpacity;         
float uProgress;        
float uIntensity;       
float uTime;            
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

    
    
    float swirlFalloff = exp(-dist * 10.0);
    float swirl = swirlFalloff * (uIntensity * 1.8);

    float pullFalloff = exp(-dist * 8.0);
    float radialPull = pullFalloff * (uIntensity * 0.07);

    
    float newAngle = angle + swirl;
    float newDist = max(dist - radialPull, 0.0);

    float2 warped;
    warped.x = cos(newAngle) * newDist;
    warped.y = sin(newAngle) * newDist;
    warped.x /= aspect;
    float2 distortedCoords = warped + center;

    
    distortedCoords = clamp(distortedCoords, 0.001, 0.999);

    
    float aberration = uIntensity * 0.006 * exp(-dist * 12.0);
    float2 abDir = float2(0.0, 0.0);
    if (dist > 0.0001)
        abDir = p / dist;
    abDir.x /= aspect;

    float3 sceneColor;
    sceneColor.r = tex2D(uImage0, distortedCoords + abDir * aberration).r;
    sceneColor.g = tex2D(uImage0, distortedCoords).g;
    sceneColor.b = tex2D(uImage0, distortedCoords - abDir * aberration).b;

    
    
    float spiralAngle = angle * 3.0 + log(max(dist, 0.001)) * 5.0 - uTime * 3.5;
    float spiralArm1 = sin(spiralAngle) * 0.5 + 0.5;
    float spiralArm2 = sin(spiralAngle * 0.5 + 1.5) * 0.5 + 0.5;
    float spiralPattern = pow(spiralArm1 * spiralArm2, 1.8);

    
    float energyFalloff = exp(-dist * (12.0 / max(uProgress, 0.01)));
    float energyMask = energyFalloff * uIntensity;

    
    float3 energyColor = lerp(uColor, uSecondaryColor, sin(uTime * 2.0 + dist * 15.0) * 0.5 + 0.5);
    energyColor *= spiralPattern * 2.0;

    
    float coreMask = SmoothStep01((dist - 0.008) / 0.015);

    
    float3 finalColor = sceneColor * coreMask + energyColor * energyMask * coreMask;

    
    float ringRadius = 0.035 * uProgress;
    float ringWidth = 0.012 * uProgress;
    float ring = 1.0 - Saturate01(abs(dist - ringRadius) / max(ringWidth, 0.001));
    ring = ring * ring * uIntensity;
    finalColor += uSecondaryColor * ring * 0.5;

    
    float coreGlow = exp(-dist * 40.0) * uIntensity;
    finalColor += uColor * coreGlow;

    
    float3 originalColor = tex2D(uImage0, coords).rgb;
    float3 outColor = lerp(originalColor, finalColor, uOpacity);

    return float4(outColor, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
