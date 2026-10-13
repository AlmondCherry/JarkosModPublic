







sampler uImage0 : register(s0);

float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageOffset;
float  uIntensity;
float  uProgress;
float  uOpacity;
float3 uColor;
float3 uSecondaryColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 sceneSample = tex2D(uImage0, coords);

    
    float wx = uScreenPosition.x + coords.x * uImageOffset.x;
    float wy = uScreenPosition.y + coords.y * uImageOffset.y;
    float dx = wx - uTargetPosition.x;
    float dy = wy - uTargetPosition.y;
    float distSq = dx * dx + dy * dy;
    float radius = uIntensity;
    float radius2 = radius * radius;
    float maskRaw = (distSq - radius2) * 0.000004;
    float mask = saturate(maskRaw);

    
    float t = uProgress;
    float row = coords.y;

    
    float phase1 = coords.x * 30.0 + t * 6.0 + row * 4.0;
    float s1 = sin(phase1) * 0.5 + 0.5;
    float strip1 = s1 * s1;

    
    float phase2 = coords.x * 26.0 - t * 7.0 + row * 3.5 + 1.7;
    float s2 = sin(phase2) * 0.5 + 0.5;
    float strip2 = s2 * s2;

    
    float hue = t * 0.5 + row * 1.2;
    float r = sin(hue) * 0.5 + 0.5;
    float g = sin(hue + 2.094) * 0.5 + 0.5;
    float b = sin(hue + 4.189) * 0.5 + 0.5;

    
    float intensity = strip1 + strip2;

    float colR = r * uColor.x * strip1 + uSecondaryColor.x * strip2;
    float colG = g * uColor.y * strip1 + uSecondaryColor.y * strip2;
    float colB = b * uColor.z * strip1 + uSecondaryColor.z * strip2;

    float effect = mask * uOpacity;

    float outR = sceneSample.x + (colR - sceneSample.x * 0.15) * effect;
    float outG = sceneSample.y + (colG - sceneSample.y * 0.15) * effect;
    float outB = sceneSample.z + (colB - sceneSample.z * 0.15) * effect;

    return float4(outR, outG, outB, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
