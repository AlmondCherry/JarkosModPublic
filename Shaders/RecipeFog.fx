sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float uTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    
    float edgeX = coords.x * (1.0 - coords.x) * 4.0;
    float edgeY = coords.y * (1.0 - coords.y) * 4.0;
    float mask = saturate(edgeX * edgeY * 4.0);
    
    
    float2 c1 = coords * 1.0 + float2(uTime * 0.1, sin(uTime*0.3)*0.05);
    float n1 = tex2D(uImage1, c1).r;
    
    float2 c2 = coords * 1.5 - float2(uTime * 0.15, cos(uTime*0.4)*0.05);
    float n2 = tex2D(uImage1, c2).r;
    
    float2 c3 = coords * 0.5 + float2(0.0, uTime * 0.05);
    float n3 = tex2D(uImage1, c3).r;
    
    
    float totalNoise = (n1 + n2 + n3) / 3.0;
    
    
    totalNoise = saturate(totalNoise * 2.5);
    
    float finalVal = totalNoise * mask;
    
    
    
    float isFog = smoothstep(0.2, 0.5, finalVal);
    
    
    float isOutline = smoothstep(0.05, 0.2, finalVal) - smoothstep(0.2, 0.4, finalVal);
    isOutline = saturate(isOutline);
    
    float alpha = saturate(isFog * 0.95 + isOutline * 1.0);
    
    
    float3 rgb = isFog * float3(0.4, 0.4, 0.4) + isOutline * float3(1.0, 1.0, 1.0);
    
    return float4(rgb * alpha, alpha) * color.a;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
