sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float4 uBumps;
float4 uBumpsActive;
float uDistance;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float y = coords.y * 2.0 - 1.0; 
    float x = coords.x;

    float realX = x * uDistance;
    float baseThickness = 0.15;
    float bumpThickness = 0.0;
    float bumpPixelWidth = 40.0;
    float bumpWidthProgress = bumpPixelWidth / uDistance;
    
    for (int i = 0; i < 4; i++) 
    {
        float bX = (i == 0) ? uBumps.x : (i == 1) ? uBumps.y : (i == 2) ? uBumps.z : uBumps.w;
        float bA = (i == 0) ? uBumpsActive.x : (i == 1) ? uBumpsActive.y : (i == 2) ? uBumpsActive.z : uBumpsActive.w;
        
        if (bA > 0.5) 
        {
            float distToBump = abs(x - bX);

            if (distToBump < bumpWidthProgress) 
            {
                float falloff = 1.0 - (distToBump / bumpWidthProgress);
                falloff = sin(falloff * 1.570796);
                falloff *= falloff;
                
                bumpThickness += 0.4 * falloff;
            }
        }
    }
    
    float waveOffset = sin(realX * 0.05 - uTime * 15.0) * 0.15;
    float distFromCenter = abs(y - waveOffset);
    
    float currentThickness = baseThickness + bumpThickness;

    if (distFromCenter < currentThickness) 
    {
        float coreWidth = currentThickness * 0.3;
        float intensity = (distFromCenter < coreWidth) ? 1.0 : 0.0;
        
        float edgeAlpha = 1.0;
        float distToEdge = currentThickness - distFromCenter;

        float softEdgeProgress = 3.0 / 20.0;
        if (distToEdge < softEdgeProgress) {
            edgeAlpha = distToEdge / softEdgeProgress;
        }
        
        float3 finalColor = lerp(uColor, float3(1.0, 1.0, 1.0), intensity);
        return float4(finalColor, 1.0) * color.a * edgeAlpha;
    }
    
    return float4(0, 0, 0, 0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
