sampler uImage0 : register(s0);



float3 uColor;

float3 uSecondaryColor;

float uOpacity;

float uSaturation;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float Hash21(float2 value)
{
    value = frac(value * float2(139.17, 337.91));
    value += dot(value, value + 31.73);
    return frac(value.x * value.y);
}

float Noise(float2 value)
{
    float2 cell = floor(value);
    float2 local = frac(value);
    local = local * local * (3.0 - 2.0 * local);

    float a = Hash21(cell);
    float b = Hash21(cell + float2(1.0, 0.0));
    float c = Hash21(cell + float2(0.0, 1.0));
    float d = Hash21(cell + float2(1.0, 1.0));

    return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
}







float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float time = uSaturation;
    
    float2 p = coords * 2.0 - 1.0;
    p.y = -p.y;
    float r2 = dot(p, p);
    if (r2 > 1.0)
        return float4(0.0, 0.0, 0.0, 0.0);

    float r = sqrt(r2 + 0.00001);
    
    float z = sqrt(Clamp01(1.0 - r2));
    float3 n = normalize(float3(p.x, p.y, z));

    
    float rotationSpeed = 0.85;
    float angleY = time * rotationSpeed;
    float sy = sin(angleY);
    float cy = cos(angleY);
    float angleX = time * rotationSpeed * 0.33;
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
    uv1.x += time * 0.025;
    uv2.x -= time * 0.018;

    float4 tex1 = tex2D(uImage0, uv1);
    float4 tex2 = tex2D(uImage0, uv2);
    float3 texColor = tex1.rgb * 0.74 + tex2.rgb * 0.26;
    float texLum = dot(texColor, float3(0.299, 0.587, 0.114));
    
    texLum = Clamp01((texLum - 0.085) * 1.85);
    float vein = pow(texLum, 1.6);

    
    
    float3 lightDir = normalize(float3(-0.35, -0.55, 0.86));
    float lambert = Clamp01(dot(n, lightDir));
    
    float fill = Clamp01(dot(n, float3(0.20, 0.55, 0.40))) * 0.22;

    
    float3 viewDir = float3(0.0, 0.0, 1.0);
    float3 halfDir = normalize(lightDir + viewDir);
    float specular = pow(Clamp01(dot(n, halfDir)), 36.0);

    
    
    float fresnel = pow(1.0 - z, 2.6);

    
    float edgeDistance = abs(r - 1.0);
    float edge = Clamp01(1.0 - edgeDistance / 0.030);
    edge *= edge;
    float thinEdge = Clamp01(1.0 - edgeDistance / 0.008);
    thinEdge *= thinEdge;

    
    float atmo = pow(Clamp01(z), 0.6);

    
    
    float3 voidBlack = float3(0.004, 0.0, 0.012);
    float3 deepCore = voidBlack + uColor * 0.06 * atmo;
    
    
    float3 surface = lerp(deepCore, uColor * 0.28 + texColor * 0.55, texLum);

    
    
    float3 color = surface * (0.22 + lambert * 0.55 + fill);
    color += uSecondaryColor * vein * 0.20;                 
    color += uSecondaryColor * fresnel * 0.30;              
    color += uColor * edge * 0.32;                          
    color += float3(0.95, 0.92, 1.00) * thinEdge * 0.40;    
    color += float3(1.0, 0.96, 1.0) * specular * 0.28;      

    
    
    float inside = Clamp01((1.0 - r) / 0.012);
    float alpha = 0.16;
    alpha += texLum * 0.32;
    alpha += fresnel * 0.32;
    alpha += edge * 0.28;
    alpha += thinEdge * 0.22;
    alpha += specular * 0.10;
    alpha *= inside * Clamp01(uOpacity) * vertexColor.a;

    return float4(Clamp01(color.r), Clamp01(color.g), Clamp01(color.b), Clamp01(alpha));
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
