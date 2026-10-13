







float uTime;
float uStack;
float uFlash;
float uZSign;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float3 col = float3(0.0, 0.0, 0.0);
    float baseAng = uTime * 2.8;

    
    float ang0 = baseAng;
    float z0 = sin(ang0);
    float2 q0 = float2(0.5 + cos(ang0) * 0.34, 0.5 + sin(ang0) * 0.115 - z0 * 0.02);
    float m0 = saturate(z0 * uZSign * 5.0 + 0.15);
    float d0 = length(coords - q0) * (8.5 - z0 * 2.2);
    float g0 = saturate(1.0 - d0);
    col += float3(1.0, 0.86, 0.38) * g0 * g0 * m0 * (1.05 + z0 * 0.3);

    float ang1 = baseAng - 0.30;
    float z1 = sin(ang1);
    float2 q1 = float2(0.5 + cos(ang1) * 0.34, 0.5 + sin(ang1) * 0.115 - z1 * 0.02);
    float m1 = saturate(z1 * uZSign * 5.0 + 0.15);
    float d1 = length(coords - q1) * (10.5 - z1 * 2.0);
    float g1 = saturate(1.0 - d1);
    col += float3(1.0, 0.74, 0.28) * g1 * g1 * m1 * 0.55;

    float ang2 = baseAng - 0.60;
    float z2 = sin(ang2);
    float2 q2 = float2(0.5 + cos(ang2) * 0.34, 0.5 + sin(ang2) * 0.115 - z2 * 0.02);
    float m2 = saturate(z2 * uZSign * 5.0 + 0.15);
    float d2 = length(coords - q2) * (13.0 - z2 * 1.8);
    float g2 = saturate(1.0 - d2);
    col += float3(1.0, 0.62, 0.20) * g2 * g2 * m2 * 0.28;

    
    col = col * (0.55 + uStack * 0.6);

    
    float dc = length(coords - float2(0.5, 0.5));
    float fl = uFlash * saturate(1.0 - dc * 1.45) * saturate(uZSign);
    fl = fl * fl;
    col += float3(1.0, 0.95, 0.72) * fl * 2.4;

    
    return float4(col, 0.0);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
