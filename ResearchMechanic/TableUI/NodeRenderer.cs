using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using jarkos.UI.Common;
using jarkos.core.Data;
using jarkos.core;
using Terraria.Audio;

namespace jarkos.UI
{
    public class NodeRenderer
    {
        private const int ParticlesPerNode = 16;
        private struct Star
        {
            public Vector2 Offset;
            public float Phase;
            public float Size;
        }
        private readonly Dictionary<string, Star[]> _stars = new();
        private readonly Dictionary<string, float> _glow = new();
        private readonly Dictionary<string, float> _edgeReveal = new();
        private readonly Dictionary<string, NodeState> _prevStates = new();
        private readonly ParticlePool _particles;
        private static readonly Random _rng = new(20240626);
        public NodeRenderer(ParticlePool particles) => _particles = particles;
        private Star[] StarsFor(ConstellationEntry e)
        {
            if (_stars.TryGetValue(e.ID, out var s)) return s;
            var rng = new Random(e.ID.GetHashCode());
            s = new Star[ParticlesPerNode];

            for (int i = 0; i < ParticlesPerNode; i++)
            {
                float ang = (float)(rng.NextDouble() * Math.PI * 2);
                float rad = (float)rng.NextDouble() * 18f + (i == 0 ? 0f : 4f);

                s[i] = new Star
                {
                    Offset = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * rad, Phase = (float)(rng.NextDouble() * Math.PI * 2), Size = i == 0 ? 2.8f : 1f + (float)rng.NextDouble() * 1.5f
                };
            }
            _stars[e.ID] = s;
            return s;
        }
        private class TreeSpark
        {
            public string FromId;
            public string ToId;
            public float Progress;
            public float Speed;
            public Color Color;
        }
        private readonly List<TreeSpark> _treeSparks = new();
        public void Update(float dt, TreeCamera cam)
        {
            var justCompleted = new HashSet<string>();
            var justRevealed = new List<string>();

            foreach (var kv in GraphState.States)
            {
                string id = kv.Key;
                NodeState now = kv.Value;
                _prevStates.TryGetValue(id, out var prev);

                if (prev != NodeState.Completed && now == NodeState.Completed)
                    justCompleted.Add(id);

                if (prev == NodeState.Hidden && now != NodeState.Hidden)
                    justRevealed.Add(id);
                _prevStates[id] = now;
            }
            foreach (string id in justRevealed)
            {
                var entry = ConstellationRegistry.Get(id);
                if (entry == null) continue;
                string sourceId = null;

                foreach (var pre in entry.Prerequisites)
                {
                    if (justCompleted.Contains(pre)) { sourceId = pre; break; }
                }

                if (sourceId == null && entry.Prerequisites.Count > 0)
                    sourceId = entry.Prerequisites[0];

                if (sourceId != null)
                {
                    _treeSparks.Add(new TreeSpark
                    {
                        FromId = sourceId, ToId = id, Progress = 0f, Speed = 0.5f + (float)_rng.NextDouble() * 1.5f, Color = EssencePalette.ColorOf(entry.Tier)
                    });
                    _edgeReveal[id] = -0.01f;
                }
                else
                {
                    _edgeReveal[id] = 0f;
                    if (entry != null)
                        _particles.Burst(entry.WorldPosition, EssencePalette.ColorOf(entry.Tier), 14, 200f, 0.5f, true);
                }
            }
            for (int i = _treeSparks.Count - 1; i >= 0; i--)
            {
                var Spark = _treeSparks[i];
                Spark.Progress += dt * Spark.Speed;
                _edgeReveal[Spark.ToId] = Math.Max(0f, Spark.Progress);

                if (Spark.Progress >= 1f)
                {
                    _edgeReveal[Spark.ToId] = 1f;
                    var entry = ConstellationRegistry.Get(Spark.ToId);
                    if (entry != null)

                    _particles.Burst(entry.WorldPosition, Spark.Color, 18, 250f, 0.6f, true);

                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NodeExtensionDone") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
                    _treeSparks.RemoveAt(i);
                }
            }
            foreach (var key in _edgeReveal.Keys.ToList())
            {
                float r = _edgeReveal[key];
                if (r >= 0f && r < 1f && !_treeSparks.Exists(s => s.ToId == key))
                {
                    _edgeReveal[key] = Math.Min(1f, r + dt / 0.45f);
                }
            }
        }
        private float StepGlow(string id, float target, float dt, float speed = 6f)
        {
            _glow.TryGetValue(id, out var g);
            g += (target - g) * Math.Min(1f, dt * speed);
            _glow[id] = g;

            return g;
        }
        public float EdgeReveal(string childId) =>
            _edgeReveal.TryGetValue(childId, out var r) ? r : 1f;
        public void DrawNode(SpriteBatch sb, ConstellationEntry e, TreeCamera cam, float time, bool hovered, float dt)
        {
            NodeState state = GraphState.Get(e.ID);

            if (state == NodeState.Hidden)
            {
                float t = StepGlow(e.ID, 0f, dt);
                if (t < 0.01f) { DrawFog(sb, e, cam, time); return; }
            }
            Color tier = EssencePalette.ColorOf(e.Tier);

            Vector2 center = cam.WorldToScreen(e.WorldPosition);
            float z = cam.Zoom;

            float baseBright = state switch
            {
                NodeState.Revealed => 0.18f, NodeState.Active => 0.55f, NodeState.Completed => 0.7f, _ => 0.05f
            };

            float target = baseBright + (hovered ? 0.85f : 0f);
            if (state == NodeState.Active) target += 0.2f * Easing.Breathe(time, 5f);
            float glow = StepGlow(e.ID, MathF.Min(1.2f, target), dt);
            if (glow > 0.05f)
                Draw.RadialGlow(sb, center, (20f + glow * 34f) * z, tier * (glow * 0.5f));

            var stars = StarsFor(e);
            Vector2[] animPos = new Vector2[stars.Length];

            for (int i = 0; i < stars.Length; i++)
            {
                float floatX = MathF.Cos(time * 0.8f + stars[i].Phase) * 4f;
                float floatY = MathF.Sin(time * 1.1f + stars[i].Phase) * 4f;
                animPos[i] = center + (stars[i].Offset + new Vector2(floatX, floatY)) * z;
            }

            if (glow > 0.1f)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    for (int j = i + 1; j < stars.Length; j++)
                    {
                        Vector2 p1 = animPos[i];
                        Vector2 p2 = animPos[j];

                        float distSq = Vector2.DistanceSquared(p1, p2);
                        float maxDist = 22f * z;
                        if (distSq < maxDist * maxDist)
                        {
                            float alpha = 1f - (MathF.Sqrt(distSq) / maxDist);
                            alpha *= 0.4f * glow;

                            float tw = 0.5f + 0.5f * MathF.Sin(time * 2f + stars[i].Phase);
                            Draw.Line(sb, p1, p2, tier * alpha * tw, 1.5f * z);
                        }
                    }
                }
            }
            for (int i = 0; i < stars.Length; i++)
            {
                var st = stars[i];
                Vector2 p = animPos[i];

                float tw = 0.45f + 0.55f * MathF.Sin(time * 2.2f + st.Phase);
                float bright = Easing.Clamp01(glow) * (0.5f + 0.5f * tw);
                float coreBoost = i == 0 ? 1.4f : 1f;
                var col = Color.Lerp(tier, Color.White, 0.3f + glow * 0.4f) * (bright * coreBoost * 0.9f);
                float size = st.Size * z * (0.8f + glow * 0.6f);

                sb.Draw(Draw.Pixel, p, null, col, 0f, new Vector2(0.5f), new Vector2(size), SpriteEffects.None, 0f);
                if (size > 1f)
                {
                    Draw.RadialGlow(sb, p, size * 4.5f, col * 0.5f);
                }
            }
            if (state == NodeState.Completed)
                Draw.RadialGlow(sb, center, 18f * z, tier * 0.4f);
        }
        private void DrawFog(SpriteBatch sb, ConstellationEntry e, TreeCamera cam, float time)
        {
            Vector2 p = cam.WorldToScreen(e.WorldPosition);
            Color tier = EssencePalette.ColorOf(e.Tier);

            float fog = 0.3f + Easing.Breathe(time, 0.9f, e.ID.GetHashCode() % 13) * 0.1f;
            Draw.RadialGlow(sb, p, 16f * cam.Zoom, new Color(20, 25, 40) * 0.8f);
            
            sb.Draw(Draw.Pixel, p, null, tier * fog * 0.5f, 0f, new Vector2(0.5f), new Vector2(3.5f * cam.Zoom), SpriteEffects.None, 0f);
        }
        public bool IsHover(ConstellationEntry e, TreeCamera cam, Vector2 mouse)
        {
            Vector2 pos = cam.WorldToScreen(e.WorldPosition);
            return Vector2.Distance(pos, mouse) < 30f * cam.Zoom;
        }
    }
}
