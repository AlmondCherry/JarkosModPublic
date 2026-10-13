sampler uImage0 : register(s0);

float uTime;
float uSeed;
float uProgress;

float hash11(float p)
{
    p = frac(p * 0.1031);
    p = p * (p + 33.33);
    return frac(p * p * 2.0);
}

float expBolt(float angle01, float dist, float time, float phase, float seed, float reach)
{
    float rayAng = hash11(phase * 13.0 + seed);
    float d = angle01 - rayAng;
    d = d - floor(d + 0.5);

    float seg = floor(dist * 8.0 + phase * 7.0);
    float jag = (hash11(seg + seed + phase * 5.0) - 0.5) * 0.10;

    float ray = pow(saturate(1.0 - abs(d + jag) * 20.0), 2.0);
    ray *= smoothstep(0.02, 0.10, dist) * (1.0 - smoothstep(reach - 0.15, reach, dist));
    ray *= step(0.12, hash11(phase + floor(time * 1.5) + seed));
    return ray;
}

float ring(float dist, float R, float sharp)
{
    return saturate(1.0 - abs(dist - R) * sharp);
}

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 dir = coords - float2(0.5, 0.5);
    float dist = sqrt(dot(dir, dir)) * 2.0;
    float angle01 = atan2(dir.y, dir.x) / 6.2831853 + 0.5;
    float time = uTime * 18.0 + uSeed;
    float fade = saturate(1.0 - uProgress);
    float fadeSmooth = sin(fade * 1.5707963);

    float wave1 = sin(angle01 * 37.699 + time * 2.5) * 0.038;
    float wave2 = cos(angle01 * 62.831 - time * 3.8) * 0.022;
    float wavyDist = dist + (wave1 + wave2) * (0.3 + uProgress * 0.7);

    float flash = pow(saturate(1.0 - dist * (1.0 + uProgress * 5.0)), 1.8) * fadeSmooth * 2.0;

    float ringR1 = uProgress * 0.92;
    float ringR2 = uProgress * 0.65;
    float ringR3 = uProgress * 0.40;

    float r1 = ring(wavyDist, ringR1, 8.5) * fadeSmooth * 1.5;
    float r2 = ring(wavyDist, ringR2, 12.0) * fadeSmooth * 0.8;
    float r3 = ring(wavyDist, ringR3, 16.0) * fadeSmooth * 0.5;

    float sector = floor(angle01 * 36.0);
    float rayNoise = hash11(sector + uSeed * 9.7);
    float rays = pow(rayNoise, 3.0) * saturate(1.0 - dist * (0.9 + uProgress * 1.5)) * smoothstep(0.02, 0.10, dist) * fadeSmooth * 1.4;

    float reach = 0.25 + uProgress * 0.85;
    float bolts = expBolt(angle01, wavyDist, time, 0.0,   uSeed,        reach);
    bolts += expBolt(angle01, wavyDist, time, 0.125, uSeed + 3.1,  reach);
    bolts += expBolt(angle01, wavyDist, time, 0.25,  uSeed + 7.9,  reach);
    bolts += expBolt(angle01, wavyDist, time, 0.375, uSeed + 11.3, reach);
    bolts += expBolt(angle01, wavyDist, time, 0.5,   uSeed + 15.7, reach);
    bolts += expBolt(angle01, wavyDist, time, 0.625, uSeed + 19.1, reach);
    bolts += expBolt(angle01, wavyDist, time, 0.75,  uSeed + 23.6, reach);
    bolts += expBolt(angle01, wavyDist, time, 0.875, uSeed + 27.2, reach);
    bolts *= fadeSmooth;

    float3 deep  = float3(0.05, 0.18, 0.65);
    float3 azure = float3(0.15, 0.75, 1.00);
    float3 white = float3(0.97, 0.99, 1.00);

    float3 finalColor = azure * (r1 + r2 + r3) * 1.6;
    finalColor += deep * rays * 0.6 + azure * rays;
    finalColor += white * bolts * 1.6 + azure * bolts * 0.6;
    finalColor = lerp(finalColor, white, saturate(flash));

    float alpha = saturate((flash + r1 * 1.1 + r2 * 0.7 + r3 * 0.4 + rays * 0.7 + bolts) * fadeSmooth);
    return float4(finalColor * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass LightningExplosionPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
