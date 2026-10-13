
sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;   
float uOpacity;
float uIntensity;         
float uProgress;          
float uTime;
float2 uZoom;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 targetUV = (uTargetPosition - uScreenPosition) / uScreenResolution;

    float aspect = uScreenResolution.x / uScreenResolution.y;
    float2 dir = coords - targetUV;
    dir.x *= aspect; 

    float dist = length(dir);
    float radius = uProgress * 0.7;   
    float width  = 0.06;              

    
    float ring = 1.0 - smoothstep(0.0, width, abs(dist - radius));

    
    float2 offset = normalize(dir + 0.0001) * ring * 0.025 * uIntensity;
    offset.x /= aspect;

    float4 color = tex2D(uImage0, coords - offset);

    
    color.rgb += float3(0.35, 0.33, 0.30) * ring * uIntensity * 0.5;

    return color;
}

technique Technique1
{
    pass ZarethShockwavePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
