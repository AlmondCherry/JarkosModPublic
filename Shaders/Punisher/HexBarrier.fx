sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uProgress;
float uIntensity;
float2 uImageOffset;
float2 uScreenPosition;
float2 uTargetPosition;

float uTime;
float2 uArenaSize; 
float2 uPlayerPos;

float Clamp01(float value)
{
    return min(1.0, max(0.0, value));
}

float hexDist(float2 p) {
    p = abs(p);
    return max(dot(p, float2(0.5, 0.8660254)), p.x);
}

float hexGrid(float2 uv) {
    float2 r = float2(1.0, 1.7320508);
    float2 h = r * 0.5;
    float2 a = frac(uv / r) * r - h;
    float2 b = frac((uv - h) / r) * r - h;
    float d1 = dot(a, a);
    float d2 = dot(b, b);
    float2 gv = lerp(a, b, step(d2, d1));
    return 0.5 - hexDist(gv);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenSize = max(uImageOffset, float2(1.0, 1.0));
    float2 worldPos = uScreenPosition + coords * screenSize;
    
    
    float2 wallVec = abs(worldPos - uTargetPosition) - uArenaSize;
    float dist = length(max(wallVec, 0.0)) + min(max(wallVec.x, wallVec.y), 0.0);
    
    
    float edgeVisibility = Clamp01(1.0 - abs(dist) / 250.0);
    
    
    float2 playerToPixel = worldPos - uPlayerPos;
    float distToPlayer = length(playerToPixel);
    float wavePhase = distToPlayer * 0.03 - uTime * 20.0;
    float wave = sin(wavePhase) * 0.5 + 0.5;
    
    
    float2 dir = playerToPixel / max(distToPlayer, 0.0001);
    float2 uv = coords + dir * (wave * 0.02 * edgeVisibility);
    uv = float2(Clamp01(uv.x), Clamp01(uv.y));
    float3 baseColor = tex2D(uImage0, uv).rgb;
    
    
    float2 uvDeep = worldPos * 0.005 + float2(uTime * 0.02, uTime * 0.03);
    float hDeep = hexGrid(uvDeep);
    float lineDeep = Clamp01(1.0 - hDeep / 0.1);
    
    
    float3 deepColor = float3(0.5, 0.1, 0.8) * lineDeep * wave;
    float3 boundaryColor = float3(1.0, 0.2, 1.0) * Clamp01(1.0 - abs(dist) / 10.0) * 2.0;
    
    float3 effectColor = deepColor + boundaryColor;
    
    
    float finalAlpha = edgeVisibility * Clamp01(uOpacity);
    float3 finalColor = lerp(baseColor, baseColor * 0.3 + effectColor, finalAlpha);
    
    return float4(finalColor, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
