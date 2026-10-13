sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float4 uBumps;
float4 uBumpsActive;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float y = coords.y * 2.0 - 1.0; 
    float x = coords.x;
    
    float dist = abs(y);
    float thickness = 0.15;
    
    if (uBumpsActive.x > 0.5) thickness += max(0.0, 0.3 * (1.0 - abs(x - uBumps.x) / 0.1));
    if (uBumpsActive.y > 0.5) thickness += max(0.0, 0.3 * (1.0 - abs(x - uBumps.y) / 0.1));
    if (uBumpsActive.z > 0.5) thickness += max(0.0, 0.3 * (1.0 - abs(x - uBumps.z) / 0.1));
    if (uBumpsActive.w > 0.5) thickness += max(0.0, 0.3 * (1.0 - abs(x - uBumps.w) / 0.1));
    
    if (dist < thickness) {
        float core = dist < (thickness * 0.3) ? 1.0 : 0.0;
        float3 finalColor = lerp(uColor, float3(1,1,1), core);
        return float4(finalColor, 1.0) * color.a;
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
