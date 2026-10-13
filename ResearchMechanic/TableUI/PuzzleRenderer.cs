using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System;
using jarkos.UI.Common;
using jarkos.core.Data;
using jarkos.core.Puzzle;
using jarkos.core.PlayerInteract;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent;

namespace jarkos.UI
{
    public class PuzzleRenderer
    {
        private ParticlePool _particles;

        public class PuzzleImpulse
        {
            public int FromNode;
            public int ToNode;
            public float Progress;
            public Color Color;
            public bool Failed;
            public float FlareScale;
        }

        private List<PuzzleImpulse> _impulses = new();

        public class StartupImpulse
        {
            public int FromNode;
            public int ToNode;
            public float Progress;
        }

        private List<StartupImpulse> _startupImpulses = new();

        public class FlyingEssence
        {
            public int TargetNodeID;
            public EssenceType Essence;
            public Vector2 StartScreenPos;
            public float Progress;
            public float CurveAmp;
            public float CurveFreq;
            public float Phase;
        }

        private List<FlyingEssence> _flyingEssences = new();
        private Dictionary<int, float> _nodeExplosionTimes = new();
        private Dictionary<int, float> _nodeRevealTimes = new();

        private PuzzleForm _currentForm;

        public PuzzleRenderer(ParticlePool particles)
        {
            _particles = particles;
        }

        public bool HasFlyingEssences(int nodeId) => _flyingEssences.Any(f => f.TargetNodeID == nodeId);

        public void SpawnFlyingEssence(int targetNodeId, EssenceType essence, Vector2 startScreenPos, float delay = 0f)
        {
            _flyingEssences.Add(new FlyingEssence
            {
                TargetNodeID = targetNodeId, Essence = essence, StartScreenPos = startScreenPos, Progress = -delay, CurveAmp = Main.rand.NextFloat(150f, 400f) * (Main.rand.NextBool() ? 1 : -1), CurveFreq = Main.rand.NextFloat(1f, 2.5f), Phase = Main.rand.NextFloat(0f, MathF.PI * 2f)
            });
        }

        public void ClearImpulses()
        {
            _impulses.Clear();
            _startupImpulses.Clear();
            _nodeRevealTimes.Clear();
            _flyingEssences.Clear();
            _nodeExplosionTimes.Clear();
        }
        
        public bool IsStartupComplete()
        {
            if (_currentForm == null) return true;
            if (_startupImpulses.Count > 0) return false;
            foreach (var rTime in _nodeRevealTimes.Values)
            {
                if (rTime < 0.6f) return false;
            }
            return true;
        }

        public void ResetStartup(PuzzleForm form)
        {
            ClearImpulses();
            _currentForm = form;
            if (form != null)
            {
                foreach (var node in form.Nodes.Where(n => n.Type == PuzzleNodeType.Start))
                {
                    _nodeRevealTimes[node.ID] = -0.5f;
                }
            }
        }

        public void FireImpulses(PuzzleForm form, int sourceId, Dictionary<int, PuzzleNodeState> currentStates)
        {
            if (form == null) return;
            foreach (var edge in form.Edges)
            {
                if (edge.FromID == sourceId)
                {
                    bool failed = currentStates.TryGetValue(edge.ToID, out var st) && st == PuzzleNodeState.Locked;
                    var fromNode = form.Nodes.FirstOrDefault(n => n.ID == sourceId);
                    Color col = fromNode != null ? EssencePalette.ColorOf(fromNode.RequiredEssence) : Color.White;
                    _impulses.Add(new PuzzleImpulse
                    {
                        FromNode = sourceId, ToNode = edge.ToID, Progress = 0f, Color = col, Failed = failed, FlareScale = 0f
                    });
                }
            }
        }

        public void Update(float dt)
        {
            var newlyRevealed = new List<int>();
            foreach (var key in _nodeRevealTimes.Keys.ToList())
            {
                float prev = _nodeRevealTimes[key];
                _nodeRevealTimes[key] = prev + dt;
                if (prev < 0.6f && _nodeRevealTimes[key] >= 0.6f)
                {
                    newlyRevealed.Add(key);
                }
            }
            if (_currentForm != null)
            {
                foreach (var id in newlyRevealed)
                {
                    foreach (var edge in _currentForm.Edges.Where(e => e.FromID == id))
                    {
                        if (!_nodeRevealTimes.ContainsKey(edge.ToID))
                        {
                            _startupImpulses.Add(new StartupImpulse { FromNode = id, ToNode = edge.ToID, Progress = 0f });
                        }
                    }
                }
            }
            for (int i = _startupImpulses.Count - 1; i >= 0; i--)
            {
                var imp = _startupImpulses[i];
                imp.Progress += dt * 3f;
                if (imp.Progress >= 1f)
                {
                    if (!_nodeRevealTimes.ContainsKey(imp.ToNode))
                    {
                        _nodeRevealTimes[imp.ToNode] = 0f;
                        SoundEngine.PlaySound(SoundID.Tink with { Volume = 0.3f, Pitch = 0.5f });
                    }
                    _startupImpulses.RemoveAt(i);
                }
            }
            foreach (var key in _nodeExplosionTimes.Keys.ToList())
            {
                _nodeExplosionTimes[key] -= dt;
                if (_nodeExplosionTimes[key] <= 0f) _nodeExplosionTimes.Remove(key);
            }
            for (int i = _flyingEssences.Count - 1; i >= 0; i--)
            {
                var fly = _flyingEssences[i];
                fly.Progress += dt * 1.3f;
                if (fly.Progress >= 1f)
                {
                    _nodeExplosionTimes[fly.TargetNodeID] = 0.5f;
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PutEssence") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
                    _flyingEssences.RemoveAt(i);
                }
            }
            for (int i = _impulses.Count - 1; i >= 0; i--)
            {
                var imp = _impulses[i];
                if (imp.FlareScale > 0f)
                {
                    imp.FlareScale -= dt * 2.5f;
                    if (imp.FlareScale <= 0f) _impulses.RemoveAt(i);
                }
                else
                {
                    imp.Progress += dt * 1.5f;
                    if (imp.Progress >= 1f)
                    {
                        imp.Progress = 1f;
                        imp.FlareScale = 1f;
                        if (imp.Failed) SoundEngine.PlaySound(SoundID.Item10);
                        else SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PutEssence") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
                    }
                }
            }
        }
        public PuzzleNodeData GetHoveredNode(PuzzleForm form, Vector2 center, float scale, Vector2 mouse)
        {
            if (form == null) return null;
            foreach (var node in form.Nodes)
            {
                Vector2 pos = center + node.LocalPosition * scale;
                if (Vector2.DistanceSquared(pos, mouse) < 30f * 30f) return node;
            }
            return null;
        }
        private float GetEdgeCurve(int fromId, int toId, bool isLit, float time, float scale)
        {
            int hash = (fromId * 397) ^ toId;
            if (isLit)
            {
                return MathF.Sin(time * 1.5f + hash * 0.1f) * 15f * scale;
            }
            return MathF.Sin(time * 0.8f + hash * 0.1f) * 6f * scale;
        }
        public void Draw(SpriteBatch sb, PuzzleForm form, Dictionary<int, PuzzleNodeState> states, ResearchPlayer plr, Vector2 center, float time, float scale, PuzzleNodeData selectedNode, PuzzleNodeData hoveredNode, float globalAlpha = 1f)
        {
            if (form == null) return;

            foreach (var edge in form.Edges)
            {
                var fromNode = form.Nodes.FirstOrDefault(n => n.ID == edge.FromID);
                var toNode = form.Nodes.FirstOrDefault(n => n.ID == edge.ToID);

                if (fromNode == null || toNode == null) continue;

                bool fromRevealed = _nodeRevealTimes.TryGetValue(fromNode.ID, out var fr) && fr >= 0.6f;
                bool toRevealed = _nodeRevealTimes.ContainsKey(toNode.ID);
                float revealAlpha = 0f;

                if (fromRevealed && toRevealed) revealAlpha = 1f;
                else
                {
                    var imp = _startupImpulses.FirstOrDefault(i => i.FromNode == edge.FromID && i.ToNode == edge.ToID);
                    if (imp != null) revealAlpha = imp.Progress;
                }

                if (revealAlpha <= 0f) continue;

                Vector2 p1 = center + fromNode.LocalPosition * scale;
                Vector2 p2 = center + toNode.LocalPosition * scale;

                bool fromLit = states.TryGetValue(fromNode.ID, out var s1) && s1 == PuzzleNodeState.Lit;
                bool toLit = states.TryGetValue(toNode.ID, out var s2) && s2 == PuzzleNodeState.Lit;

                float curve = GetEdgeCurve(edge.FromID, edge.ToID, fromLit && toLit, time, scale);

                Color edgeColor = (fromLit && toLit) ? EssencePalette.ColorOf(toNode.RequiredEssence) : new Color(40, 40, 50);
                float thickness = (fromLit && toLit) ? 4f : 2f;
                float alpha = ((fromLit && toLit) ? 0.8f : 0.4f) * revealAlpha * globalAlpha;

                if (!fromRevealed || !toRevealed)
                {
                    Vector2 tipPos = Common.Draw.GetCurvePoint(p1, p2, MathSystem.EaseOutCubic(revealAlpha), curve);
                    Common.Draw.RadialGlow(sb, tipPos, 15f * scale, new Color(150, 180, 255) * revealAlpha * globalAlpha);
                }

                Common.Draw.GradientLine(sb, p1, p2, edgeColor * alpha, thickness, revealAlpha, 16, curve);

                if (fromLit && !toLit && states.TryGetValue(toNode.ID, out var s3) && s3 == PuzzleNodeState.Available)
                {
                    float t = (time * 0.5f + edge.FromID * 0.1f) % 1f;
                    Vector2 sparkPos = Common.Draw.GetCurvePoint(p1, p2, t, curve);
                    Common.Draw.RadialGlow(sb, sparkPos, 15f * scale, edgeColor * 0.6f * revealAlpha * globalAlpha);
                }
            }

            foreach (var imp in _impulses)
            {
                var fromNode = form.Nodes.FirstOrDefault(n => n.ID == imp.FromNode);
                var toNode = form.Nodes.FirstOrDefault(n => n.ID == imp.ToNode);

                if (fromNode == null || toNode == null) continue;

                Vector2 p1 = center + fromNode.LocalPosition * scale;
                Vector2 p2 = center + toNode.LocalPosition * scale;

                bool fromLit = states.TryGetValue(imp.FromNode, out var s1) && s1 == PuzzleNodeState.Lit;
                bool toLit = states.TryGetValue(imp.ToNode, out var s2) && s2 == PuzzleNodeState.Lit;
                float curve = GetEdgeCurve(imp.FromNode, imp.ToNode, fromLit && toLit, time, scale);

                if (imp.FlareScale > 0f)
                {
                    Color fcol = imp.Failed ? Color.Red : imp.Color;
                    
                    Common.Draw.RadialGlow(sb, p2, 80f * imp.FlareScale * scale, fcol * imp.FlareScale * globalAlpha);
                    Common.Draw.RadialGlow(sb, p2, 40f * imp.FlareScale * scale, Color.White * imp.FlareScale * globalAlpha);
                }
                else
                {
                    float curvedProg = Easing.EaseInCubic(imp.Progress);

                    Vector2 pos = Common.Draw.GetCurvePoint(p1, p2, curvedProg, curve);
                    Color c = imp.Failed && imp.Progress > 0.7f ? Color.Lerp(imp.Color, Color.Red, (imp.Progress - 0.7f) * 3.33f) : imp.Color;

                    Common.Draw.RadialGlow(sb, pos, 25f * scale, c * globalAlpha);
                    Vector2 tail = Common.Draw.GetCurvePoint(p1, p2, Easing.EaseInCubic(MathF.Max(0, imp.Progress - 0.2f)), curve);

                    Common.Draw.GradientLine(sb, tail, pos, c * 0.8f * globalAlpha, 4f * scale, 1f, 8, curve);
                }
            }
            foreach (var node in form.Nodes)
            {
                if (!_nodeRevealTimes.TryGetValue(node.ID, out float rTime)) continue;
                if (rTime < 0f) continue;

                float flickerAlpha = 1f;

                if (rTime < 0.6f)
                {
                    float noise = MathF.Sin(rTime * 50f + node.ID * 10f) * MathF.Cos(rTime * 25f - node.ID * 5f);
                    flickerAlpha = noise > 0.2f ? 1f : 0.1f;
                }

                flickerAlpha *= globalAlpha;
                Vector2 pos = center + node.LocalPosition * scale;

                var state = states.TryGetValue(node.ID, out var s) ? s : PuzzleNodeState.Locked;
                Color baseCol = EssencePalette.ColorOf(node.RequiredEssence);

                bool isSelected = selectedNode == node;
                bool isHovered = hoveredNode == node;

                if (state == PuzzleNodeState.Locked)
                {
                    Common.Draw.RadialGlow(sb, pos, 14f * scale, new Color(30, 30, 40) * flickerAlpha);
                    Terraria.Utils.DrawBorderString(sb, "?", pos - new Vector2(4 * scale, 8 * scale), Color.Gray * flickerAlpha, 0.8f * scale);
                }
                else if (state == PuzzleNodeState.Available)
                {
                    float pulse = Easing.Breathe(time, 2f);
                    float ringSize = 18f + 5f * pulse;

                    if (isHovered) ringSize += 5f;
                    if (isSelected) ringSize += 8f;

                    Common.Draw.RadialGlow(sb, pos, ringSize * scale, baseCol * (isSelected ? 0.8f : 0.4f) * flickerAlpha);
                    Common.Draw.RadialGlow(sb, pos, 10f * scale, baseCol * (isSelected ? 1f : 0.8f) * flickerAlpha);

                    if (isSelected)
                    {
                        float rot = time * 2f;
                        sb.End();
                        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                        
                        Common.Draw.RadialGlow(sb, pos, 28f * scale, Color.White * 0.3f * flickerAlpha);
                        sb.End();
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                    }
                }
                else if (state == PuzzleNodeState.Lit)
                {
                    bool overpaid = false;
                    if (plr.ActivePuzzleProgress.TryGetValue(node.ID, out int paidType))
                    {
                        overpaid = paidType != (int)node.RequiredEssence;
                    }
                    if (overpaid)
                    {
                        Color paidCol = EssencePalette.ColorOf((EssenceType)paidType);
                        float tremorX = MathF.Sin(time * 20f + node.ID) * 2f * scale;
                        float tremorY = MathF.Cos(time * 18f + node.ID) * 2f * scale;

                        pos += new Vector2(tremorX, tremorY);
                        Common.Draw.RadialGlow(sb, pos, 24f * scale, paidCol * 0.5f * flickerAlpha);
                        Common.Draw.RadialGlow(sb, pos, 12f * scale, baseCol * 0.9f * flickerAlpha);
                    }
                    else
                    {
                        Common.Draw.RadialGlow(sb, pos, 28f * scale, baseCol * 0.6f * flickerAlpha);
                        Common.Draw.RadialGlow(sb, pos, 14f * scale, baseCol * flickerAlpha);
                    }
                }
                if (_nodeExplosionTimes.TryGetValue(node.ID, out float explTime))
                {
                    float explScale = explTime / 0.5f;
                    Common.Draw.RadialGlow(sb, pos, (30f + 50f * explScale) * scale, baseCol * explScale * globalAlpha);
                    Common.Draw.RadialGlow(sb, pos, (15f + 25f * explScale) * scale, Color.White * explScale * globalAlpha);
                }
                if (state != PuzzleNodeState.Locked)
                {
                    string label = "";
                    if (node.Type == PuzzleNodeType.AND) label = "&";
                    else if (node.Type == PuzzleNodeType.OR) label = "|";
                    else if (node.Type == PuzzleNodeType.Final) label = "F";

                    if (label != "")
                    {
                        var ts = FontAssets.MouseText.Value.MeasureString(label);
                        Terraria.Utils.DrawBorderString(sb, label, pos - ts * 0.35f * scale, Color.White * 0.8f * flickerAlpha, 0.7f * scale);
                    }
                }
            }
            foreach (var fly in _flyingEssences)
            {
                if (fly.Progress < 0f) continue;
                var targetNode = form.Nodes.FirstOrDefault(n => n.ID == fly.TargetNodeID);

                if (targetNode == null) continue;

                Vector2 targetPos = center + targetNode.LocalPosition * scale;
                float easeProg = MathSystem.EaseOutCubic(fly.Progress);
                Vector2 dir = targetPos - fly.StartScreenPos;
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                if (perp != Vector2.Zero) perp.Normalize();

                float envelope = MathF.Sin(fly.Progress * MathF.PI);
                float wave = MathF.Sin(fly.Progress * MathF.PI * fly.CurveFreq + fly.Phase);

                Vector2 pos = Vector2.Lerp(fly.StartScreenPos, targetPos, easeProg) + perp * wave * envelope * fly.CurveAmp;
                Texture2D tex = EssencePalette.GetTexture(fly.Essence);
                
                if (tex != null)
                {
                    float rot = fly.Progress * MathF.PI * 4f;
                    sb.Draw(tex, pos, null, Color.White * globalAlpha, rot, tex.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
                    Common.Draw.RadialGlow(sb, pos, 25f, EssencePalette.ColorOf(fly.Essence) * 0.6f * globalAlpha);
                }
            }
        }
    }
}
