sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uProgress;
float uIntensity;
float uSaturation;

float Clamp01(float v)
{
    return min(1.0, max(0.0, v));
}

float Pulse(float v, float center, float width)
{
    float delta = v - center;
    return Clamp01(1.0 - sqrt(delta * delta + 0.00001) / width);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float seed = uSaturation;
    float time = seed * 6.2832;

    
    float2 lensDelta = coords - float2(0.5, 0.5);
    float lensDist = length(lensDelta) + 0.0001;
    float openCurve = Clamp01(uProgress / 0.20) * (1.0 - Clamp01((uProgress - 0.78) / 0.22));
    float lensStrength = (0.020 + uIntensity * 0.012) * openCurve;
    float2 warp = float2(sin(coords.y * 6.5 + time * 0.55), cos(coords.x * 5.2 - time * 0.42)) * 0.012;
    float2 lensUv = coords - (lensDelta / lensDist) * lensStrength * (1.0 - Clamp01(lensDist / 0.45)) + warp;

    float4 mask = tex2D(uImage0, lensUv);
    float maskAlpha = mask.a;
    float opacity = Clamp01(uOpacity) * vertexColor.a;
    float open = Clamp01(uProgress / 0.10) * (1.0 - Clamp01((uProgress - 0.92) / 0.08));

    
    float radial = 1.0 - Clamp01(lensDist / 0.50);
    float core = radial * radial;
    float deepCore = core * core;
    float halo = Clamp01(1.0 - lensDist / 0.55);

    
    float rimDist = abs(lensDist - 0.32);
    float rim = Clamp01(1.0 - rimDist / 0.045);
    float rimSwirl = sin(atan2(lensDelta.y, lensDelta.x) * 5.0 + time * 0.9) * 0.5 + 0.5;
    rim = rim * (0.55 + rimSwirl * 0.45);

    
    float pX = 0.62 + sin(time * 0.30) * 0.018;
    float pY = 0.44 + cos(time * 0.24) * 0.014;
    float2 planetP = (coords - float2(pX, pY)) / float2(0.045, 0.062);
    float planet = Clamp01(1.0 - sqrt(planetP.x * planetP.x + planetP.y * planetP.y)) * core;

    
    float starDx = sin(time * 0.40) * 0.020;
    float starDy = cos(time * 0.33) * 0.015;
    float twinkle = 0.65 + 0.35 * sin(time * 5.0);
    float starA = Clamp01(1.0 - sqrt((coords.x - 0.34 - starDx) * (coords.x - 0.34 - starDx) + (coords.y - 0.38 + starDy) * (coords.y - 0.38 + starDy)) / 0.011);
    float starB = Clamp01(1.0 - sqrt((coords.x - 0.72 + starDx) * (coords.x - 0.72 + starDx) + (coords.y - 0.58 - starDy) * (coords.y - 0.58 - starDy)) / 0.010);
    float starC = Clamp01(1.0 - sqrt((coords.x - 0.46 - starDy) * (coords.x - 0.46 - starDy) + (coords.y - 0.64 + starDx) * (coords.y - 0.64 + starDx)) / 0.008);
    float stars = (starA + starB + starC) * twinkle;

    
    
    float2 cp;
    cp.x = (coords.x - 0.5) * 1.10;
    cp.y = (coords.y - 0.5) * 2.00;
    float crackOffset = sin(cp.x * 8.5 + time * 0.55) * 0.022
                      + sin(cp.x * 17.0 - time * 0.85) * 0.013;
    float crackDy = abs(cp.y - crackOffset);
    float crackAlong = Clamp01(1.0 - abs(cp.x) / 0.55);
    float crackPulse = 0.85 + sin(time * 4.6) * 0.15;
    float crackHot  = Clamp01(1.0 - crackDy / (0.030 * crackPulse)) * crackAlong;
    float crackGlow = Clamp01(1.0 - crackDy / (0.090 * crackPulse)) * crackAlong;
    float crackEdge = Clamp01(1.0 - crackDy / (0.180 * crackPulse)) * crackAlong;
    crackEdge = crackEdge * crackEdge;

    
    float flash1 = Pulse(uProgress, 0.18, 0.10);
    float flash2 = Pulse(uProgress, 0.45, 0.06);
    float flash3 = Pulse(uProgress, 0.72, 0.05);
    float flashCore = flash1 * 0.85 + flash2 * 0.55 + flash3 * 0.50;

    float alphaShape = Clamp01(maskAlpha * 1.15 + core * 0.40 + halo * 0.20 + rim * 0.22 + crackGlow * 0.55);
    float lumens = core * 0.55
                 + deepCore * 0.50
                 + halo * 0.30
                 + rim * 0.50
                 + planet * 0.50
                 + stars * 0.95
                 + crackHot * 1.10
                 + crackGlow * 0.55
                 + crackEdge * 0.30
                 + flashCore * (core * 0.55 + rim * 0.40 + crackHot * 0.65);
    float alpha = Clamp01(alphaShape * lumens) * opacity * open;

    float3 color = float3(0.012, 0.0, 0.030) * alphaShape;
    color = color + uColor * (core * 0.55 + halo * 0.20 + rim * 0.40 + crackEdge * 0.40);
    color = color + uSecondaryColor * (deepCore * 0.55 + rim * 0.30 + crackGlow * 0.65);
    color = color + float3(0.92, 0.58, 1.0) * (deepCore * 0.18 + stars * 1.10 + crackHot * 0.85 + flashCore * core * 0.45);
    color = color + float3(0.96, 0.92, 1.0) * (rim * 0.32 + stars * 0.30 + crackHot * 0.55);
    color = color + float3(0.08, 0.20, 0.62) * planet * 0.85;

    
    color = color * (1.0 - deepCore * 0.42);

    return float4(Clamp01(color.r), Clamp01(color.g), Clamp01(color.b), alpha);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
