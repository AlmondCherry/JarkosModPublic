sampler uImage0 : register(s0);
 
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float4 uShaderSpecificData;
 
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 p = coords * 2.0 - 1.0;
    p.y = -p.y;
 
    float r2 = dot(p, p);
    float r = sqrt(r2 + 0.00001);
 
    float inside = 1.0 - r;
    inside = clamp(inside / 0.010, 0.0, 1.0);
 
    float z = sqrt(clamp(1.0 - r2, 0.0, 1.0));
 
    float3 n;
    n.x = p.x;
    n.y = p.y;
    n.z = z;
 
    float rotationSpeed = uShaderSpecificData.x;
    float textureStrength = uShaderSpecificData.y;
    float edgeStrength = uShaderSpecificData.z;
    float centerCutout = uShaderSpecificData.w;
 
    float angleY = uTime * rotationSpeed;
    float sy = sin(angleY);
    float cy = cos(angleY);
 
    float angleX = uTime * rotationSpeed * 0.33;
    float sx = sin(angleX);
    float cx = cos(angleX);
 
    float3 n1;
    n1.x = n.x * cy - n.z * sy;
    n1.y = n.y;
    n1.z = n.x * sy + n.z * cy;
 
    float3 n2;
    n2.x = n1.x;
    n2.y = n1.y * cx - n1.z * sx;
    n2.z = n1.y * sx + n1.z * cx;
 
    float2 uv1 = n2.xy * 0.5 + 0.5;
    float2 uv2;
    uv2.x = n2.z * 0.5 + 0.5;
    uv2.y = n2.y * 0.5 + 0.5;
 
    uv1.x = uv1.x + uTime * 0.025;
    uv2.x = uv2.x - uTime * 0.018;
 
    float4 tex1 = tex2D(uImage0, uv1);
    float4 tex2 = tex2D(uImage0, uv2);
 
    float3 texColor = tex1.rgb * 0.78 + tex2.rgb * 0.22;
    float texLum = dot(texColor, float3(0.299, 0.587, 0.114));
 
    float light = n.x * -0.32 + n.y * -0.45 + n.z * 0.92;
    light = clamp(light, 0.0, 1.0);
 
    float rim = 1.0 - z;
    rim = clamp(rim, 0.0, 1.0);
    rim = rim * rim;
 
    float edgeDistance = r - 1.0;
    edgeDistance = sqrt(edgeDistance * edgeDistance + 0.000001);
 
    float edge = 1.0 - edgeDistance / 0.022;
    edge = clamp(edge, 0.0, 1.0);
    edge = edge * edge;
 
    float orangeEdge = 1.0 - edgeDistance / 0.006;
    orangeEdge = clamp(orangeEdge, 0.0, 1.0);
    orangeEdge = orangeEdge * orangeEdge;
 
    float highlight = n.x * -0.55 + n.y * -0.70 + n.z * 1.00;
    highlight = clamp(highlight, 0.0, 1.0);
    highlight = highlight * highlight;
    highlight = highlight * highlight;
 
    float3 baseTint = uColor * 0.22 + texColor * 0.78;
 
    float3 color = baseTint * (0.34 + light * 0.66);
    color = color + uSecondaryColor * rim * 0.28 * edgeStrength;
    color = color + float3(1.0, 0.40, 0.06) * orangeEdge * edgeStrength;
    color = color + float3(1.0, 0.82, 0.55) * highlight * 0.20;
 
    float alpha = 0.20;
    alpha = alpha + texLum * 0.42 * textureStrength;
    alpha = alpha + rim * 0.22 * edgeStrength;
    alpha = alpha + edge * 0.34 * edgeStrength;
    alpha = alpha + orangeEdge * 0.35 * edgeStrength;
    alpha = alpha + highlight * 0.10;
 
    float cutoutX = p.x / 0.34;
    float cutoutY = p.y / 0.60;
    float cutout = 1.0 - clamp(cutoutX * cutoutX + cutoutY * cutoutY, 0.0, 1.0);
    cutout = cutout * cutout;
 
    alpha = alpha * (1.0 - cutout * centerCutout);
    alpha = alpha * inside * uOpacity;
    alpha = clamp(alpha, 0.0, 1.0);
 
    return float4(clamp(color, 0.0, 1.0), alpha);
}
 
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}