using System;
using Terraria.ModLoader;
namespace jarkos
{
    public class Noises
    {
        public class PerlinNoise
        {
            private readonly int[] permutation;
            private readonly int[] p;
            private readonly Random random;
            public int Seed { get; private set; }
            public int Octaves { get; set; } = 4;
            public double Persistence { get; set; } = 0.5;
            public double Lacunarity { get; set; } = 2.0;
            public PerlinNoise(int seed)
            {
                Seed = seed;
                random = new Random(seed);
                
                permutation = new int[256];
                p = new int[512];

                for (int i = 0; i < 256; i++)
                    permutation[i] = i;

                for (int i = 255; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
                }

                for (int i = 0; i < 512; i++)
                    p[i] = permutation[i & 255];
            }
            public double Noise(double x, double y)
            {
                int xi = (int)Math.Floor(x) & 255;
                int yi = (int)Math.Floor(y) & 255;
                double xf = x - Math.Floor(x);
                double yf = y - Math.Floor(y);
                double u = Fade(xf);
                double v = Fade(yf);
                int aa = p[p[xi] + yi];
                int ab = p[p[xi] + yi + 1];
                int ba = p[p[xi + 1] + yi];
                int bb = p[p[xi + 1] + yi + 1];
                double x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
                double x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

                return Lerp(x1, x2, v);
            }
            public double FractalNoise(double x, double y, int octaves = -1, double persistence = -1, double lacunarity = -1)
            {
                octaves = octaves > 0 ? octaves : Octaves;
                persistence = persistence > 0 ? persistence : Persistence;
                lacunarity = lacunarity > 0 ? lacunarity : Lacunarity;
                double total = 0;
                double amplitude = 1;
                double frequency = 1;
                double maxValue = 0;

                for (int i = 0; i < octaves; i++)
                {
                    total += Noise(x * frequency, y * frequency) * amplitude;
                    maxValue += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                return total / maxValue;
            }
            public double Turbulence(double x, double y, int octaves = 4)
            {
                double total = 0;
                double amplitude = 1;
                double frequency = 1;

                for (int i = 0; i < octaves; i++)
                {
                    total += Math.Abs(Noise(x * frequency, y * frequency)) * amplitude;
                    amplitude *= 0.5;
                    frequency *= 2;
                }

                return total;
            }
            public double RidgedNoise(double x, double y, int octaves = 4)
            {
                double total = 0;
                double amplitude = 1;
                double frequency = 1;
                double weight = 1;

                for (int i = 0; i < octaves; i++)
                {
                    double signal = 1.0 - Math.Abs(Noise(x * frequency, y * frequency));
                    signal *= signal * weight;
                    weight = Math.Min(1, Math.Max(0, signal * 2));
                    total += signal * amplitude;
                    amplitude *= 0.5;
                    frequency *= 2;
                }

                return total;
            }
            public double WarpedNoise(double x, double y, double warpStrength = 0.5)
            {
                double warpX = FractalNoise(x + 100, y + 100, 2);
                double warpY = FractalNoise(x + 200, y + 200, 2);

                return FractalNoise(x + warpX * warpStrength * 10, y + warpY * warpStrength * 10);
            }
            private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
            private static double Lerp(double a, double b, double t) => a + t * (b - a);
            private static double Grad(int hash, double x, double y)
            {
                int h = hash & 3;
                double u = h < 2 ? x : y;
                double v = h < 2 ? y : x;
                
                return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
            }
        }
    }
}
