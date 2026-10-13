using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace jarkos.UI.Common
{
    public struct Spark
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Life;
        public float MaxLife;
        public Color Color;
        public float Size;
        public bool IsWorld;
        public bool Alive => Life > 0f;
        public void Update(float dt)
        {
            Pos += Vel * dt;
            Vel *= 0.92f;
            Life -= dt;
        }
        public float Alpha => MathF.Max(0f, Life / MaxLife);
    }
    public class ParticlePool
    {
        private readonly List<Spark> _sparks = new();
        private static readonly Random _rng = new();
        public void Emit(Vector2 pos, Vector2 vel, Color color, float life, float size, bool isWorld = false)
        {
            _sparks.Add(new Spark            {
                Pos = pos, Vel = vel, Color = color, Life = life, MaxLife = life, Size = size, IsWorld = isWorld
            });
        }
        public void Burst(Vector2 pos, Color color, int count = 18, float speed = 240f, float life = 0.5f, bool isWorld = false)
        {
            for (int i = 0; i < count; i++)
            {
                float ang = MathHelper.TwoPi * i / count + (float)_rng.NextDouble() * 0.3f;
                float s = speed * (0.6f + (float)_rng.NextDouble() * 0.6f);
                Emit(pos, new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * s, color, life * (0.7f + (float)_rng.NextDouble() * 0.6f), 2f + (float)_rng.NextDouble() * 2f, isWorld);
            }
        }
        public void Update(float dt)
        {
            for (int i = _sparks.Count - 1; i >= 0; i--)
            {
                var s = _sparks[i];
                s.Update(dt);
                if (!s.Alive) _sparks.RemoveAt(i);
                else _sparks[i] = s;
            }
        }
        public void Draw(SpriteBatch sb, Texture2D pixel, Func<Vector2, Vector2> worldToScreen = null, float zoom = 1f)
        {
            foreach (var s in _sparks)
            {
                var c = s.Color * s.Alpha;
                Vector2 drawPos = s.Pos;
                float sz = s.Size;
                if (s.IsWorld && worldToScreen != null)
                {
                    drawPos = worldToScreen(s.Pos);
                    sz *= zoom;
                }
                sb.Draw(pixel, drawPos, null, c, 0f, new Vector2(0.5f), new Vector2(sz), SpriteEffects.None, 0f);
            }
        }
        public void Clear() => _sparks.Clear();
        public int Count => _sparks.Count;
    }
}
