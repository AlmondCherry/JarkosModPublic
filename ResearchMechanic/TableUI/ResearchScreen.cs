using Terraria.ID;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.Linq;
using jarkos.UI.Common;
using jarkos.core.PlayerInteract;
using jarkos.core.Data;
using jarkos.core;
using jarkos.core.Puzzle;
using Terraria.Audio;

namespace jarkos.UI
{
    public class ResearchScreen
    {
        public static ResearchScreen Instance { get; } = new();
        public bool Active { get; private set; }

        private float _time;
        private float _openProgress = 1f;
        private bool _isClosing = false;
        private float _closeProgress = 0f;

        private readonly TreeCamera _cam = new();

        private readonly CosmicBackground _bg = new();

        private readonly ParticlePool _particles = new();
        private readonly NodeRenderer _nodes;
        private readonly EssencePanel _panel;
        private readonly ConfirmPopup _popup = new();
        
        private readonly WarpTransition _warp = new();

        private readonly PuzzleRenderer _puzzleRenderer;

        public readonly BookAnimationManager _bookAnim = new();

        private bool _inPuzzleStub;
        private float _puzzleTime;
        private string _warpReturnNodeId;

        private PuzzleNodeData _selectedPuzzleNode = null;

        private bool _prevMouseLeft;
        private bool _prevMouseRight;

        private class PendingPayment
        {
            public int NodeID;
            public EssenceType Type;
        }

        private List<PendingPayment> _pendingPayments = new();
        private Vector2 _warpStartOffset;

        private float _warpStartZoom;
        private Vector2 _warpFocusWorld;

        private float _preWarpZoom;
        private Vector2 _preWarpOffset;

        private ResearchScreen()
        {
            _nodes = new NodeRenderer(_particles);
            _panel = new EssencePanel(_particles);
            _puzzleRenderer = new PuzzleRenderer(_particles);
            
            _warp.OnForwardPeak += () =>
            {
                _inPuzzleStub = true;
                _puzzleTime = 0f;
                _selectedPuzzleNode = null;

                _panel.SetHidden(true, true);

                var form = ConstellationRegistry.TryGet(_warpReturnNodeId, out var entry) ? PuzzleRegistry.Get(entry.PuzzleShapeID) : null;
                _puzzleRenderer.ResetStartup(form);

                int minX = -400, minY = -400, maxX = 400, maxY = 400;

                if (form != null && form.Nodes != null && form.Nodes.Count > 0)
                {
                    minX = int.MaxValue; minY = int.MaxValue;
                    maxX = int.MinValue; maxY = int.MinValue;

                    foreach (var pn in form.Nodes)
                    {
                        if (pn.LocalPosition.X < minX) minX = (int)pn.LocalPosition.X;
                        if (pn.LocalPosition.X > maxX) maxX = (int)pn.LocalPosition.X;
                        if (pn.LocalPosition.Y < minY) minY = (int)pn.LocalPosition.Y;
                        if (pn.LocalPosition.Y > maxY) maxY = (int)pn.LocalPosition.Y;
                    }

                    int padding = 350;
                    minX -= padding;
                    minY -= padding;
                    maxX += padding;
                    maxY += padding;
                }

                if (maxX < minX) maxX = minX;
                if (maxY < minY) maxY = minY;

                _cam.SetBounds(new Rectangle(minX, minY, maxX - minX, maxY - minY));
                _cam.Offset = Vector2.Zero;
                _cam.Zoom = 0.5f;
                _cam.TargetZoom = 1.2f;
            };
        }
        public void Open(Player player)
        {
            if (Active) return;
            Active = true;

            bool screenFlashes = Jarkos_config.ScreenFlashesEnabled;

            _openProgress = screenFlashes ? 0f : 1f;
            _prevMouseLeft = Main.mouseLeft;
            _prevMouseRight = Main.mouseRight;
            _cam.SetBounds(ConstellationRegistry.ComputeBounds());

            var plr = player.GetModPlayer<ResearchPlayer>();

            GraphState.RecomputeAll(plr);

            FocusInitial(plr);
            _panel.SetHidden(false, true);

            Main.playerInventory = false;

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ResearchTableEnter") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
        }
        public void Close()
        {
            if (_warp.Busy || !Active || _isClosing) return;

            if (!Jarkos_config.ScreenFlashesEnabled)
            {
                _popup.Hide();
                _panel.SetHidden(true, true);
                _isClosing = false;

                Active = false;

                _particles.Clear();
                _inPuzzleStub = false;

                ResearchUISystem.CloseCooldown = 15;

                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ResearchTableClose") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
                return;
            }

            _isClosing = true;
            _closeProgress = 0f;

            _popup.Hide();
            _panel.SetHidden(true);
            _cam.TargetZoom = 3.5f;

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ResearchTableClose") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
        }
        public void TriggerNodeCompletion(string nodeID)
        {
            if (!Active) return;
            if (_inPuzzleStub && _warpReturnNodeId == nodeID)
            {
                if (_puzzleTime < 100f)
                {
                    _puzzleTime = 100f;
                    _bookAnim.Trigger(nodeID, new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f));

                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.5f });
                }
            }
            else
            {
                var plr = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                GraphState.RecomputeAll(plr);
            }
        }
        public void OnPuzzleProgressReceived(int nodeId, int type)
        {
            if (!Active || !_inPuzzleStub) return;
            var form = ConstellationRegistry.TryGet(_warpReturnNodeId, out var entry) ? PuzzleRegistry.Get(entry.PuzzleShapeID) : null;

            if (form != null)
            {
                var plr = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                var states = PuzzleStateResolver.Evaluate(form, plr);

                _puzzleRenderer.FireImpulses(form, nodeId, states);

                var finalNode = form.Nodes.FirstOrDefault(n => n.Type == PuzzleNodeType.Final);

                if (finalNode != null && states.TryGetValue(finalNode.ID, out var fs) && fs == PuzzleNodeState.Lit)
                {
                    plr.CompleteResearch(_warpReturnNodeId);
                    if (_puzzleTime < 100f)
                    {
                        _puzzleTime = 100f;
                        _bookAnim.Trigger(_warpReturnNodeId, new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f));

                        SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.5f });
                    }
                }
            }
        }
        private void FocusInitial(ResearchPlayer plr)
        {
            ConstellationEntry focus = null;

            if (plr.HasActive) ConstellationRegistry.TryGet(plr.ActiveResearchID, out focus);

            if (focus == null)
                foreach (var e in ConstellationRegistry.All.Values)
                    if (e.IsRoot) { focus = e; break; }

            if (focus != null) _cam.Offset = focus.WorldPosition;
            
            _cam.Zoom = 3.5f;
            _cam.TargetZoom = 1f;
        }
        private void BeginWarpForward(ConstellationEntry node)
        {
            _warpReturnNodeId = node.ID;
            _warpFocusWorld = node.WorldPosition;

            _preWarpZoom = _cam.TargetZoom;
            _preWarpOffset = _cam.Offset;
            _warpStartOffset = _cam.Offset;
            _warpStartZoom = _cam.Zoom;

            _panel.SetHidden(true);
            _warp.BeginForward(node.WorldPosition, _cam.WorldToScreen(node.WorldPosition), EssencePalette.ColorOf(node.Tier));
        }
        private void BeginWarpReverse(Color? overrideColor = null)
        {
            if (!ConstellationRegistry.TryGet(_warpReturnNodeId, out var rn)) return;

            _inPuzzleStub = false;
            _selectedPuzzleNode = null;

            _panel.SetHidden(false);
            _warpFocusWorld = rn.WorldPosition;

            _cam.SetBounds(ConstellationRegistry.ComputeBounds());

            _cam.TargetZoom = _preWarpZoom;
            _cam.Offset = _preWarpOffset;
            _warpStartOffset = _preWarpOffset;
            _warpStartZoom = _preWarpZoom;

            Color flashCol = overrideColor ?? EssencePalette.ColorOf(rn.Tier);
            _warp.BeginReverse(rn.WorldPosition, _cam.WorldToScreen(rn.WorldPosition), flashCol);
        }
        private float _graphUpdateDelay = -1f;
        public void Update(GameTime gt, Player player)
        {
            if (!Active) return;

            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            _time += dt;

            var plr = player.GetModPlayer<ResearchPlayer>();

            if (_inPuzzleStub)
            {
                _puzzleTime += dt;
                _puzzleRenderer.Update(dt);

                var form = ConstellationRegistry.TryGet(_warpReturnNodeId, out var entry) ? PuzzleRegistry.Get(entry.PuzzleShapeID) : null;

                if (form != null)
                {
                    for (int i = _pendingPayments.Count - 1; i >= 0; i--)
                    {
                        var p = _pendingPayments[i];

                        if (!_puzzleRenderer.HasFlyingEssences(p.NodeID))
                        {
                            plr.SetPuzzleProgress(p.NodeID, (int)p.Type);
                            var states = PuzzleStateResolver.Evaluate(form, plr);
                            
                            _puzzleRenderer.FireImpulses(form, p.NodeID, states);
                            var finalNode = form.Nodes.FirstOrDefault(n => n.Type == PuzzleNodeType.Final);

                            if (finalNode != null && states.TryGetValue(finalNode.ID, out var fs) && fs == PuzzleNodeState.Lit)
                            {
                                plr.CompleteResearch(_warpReturnNodeId);
                                _puzzleTime = 100f;
                                _bookAnim.Trigger(_warpReturnNodeId, new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f));

                                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.5f });
                            }
                            _pendingPayments.RemoveAt(i);
                        }
                    }
                }
                if (_puzzleTime > 100f)
                {
                    float compT = _puzzleTime - 100f;
                    float oldCompT = compT - dt;

                    if (compT < 1.5f)
                    {
                        _cam.Offset = Vector2.Lerp(_cam.Offset, Vector2.Zero, dt * 4f);
                    }

                    if (oldCompT < 1.5f && compT >= 1.5f)
                    {
                        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 1.5f, Pitch = -0.3f });
                    }
                }
            }

            var mouse = new Vector2(Main.mouseX, Main.mouseY);

            _bg.Update(dt);
            _particles.Update(dt);
            _panel.Update(dt);
            _popup.Update(dt);
            _bookAnim.Update(dt);

            if (_graphUpdateDelay >= 0f && !_warp.Busy)
            {
                _graphUpdateDelay -= dt;
                if (_graphUpdateDelay < 0f)
                {
                    GraphState.RecomputeAll(plr);
                }
            }

            _nodes.Update(dt, _cam);

            if (_warp.Busy) ApplyWarpCamera();

            _warp.FocusScreen = _cam.WorldToScreen(_warpFocusWorld);
            _warp.Update(dt);
            _panel.RequestAttention(plr.HasActive && !_panel.Expanded);

            if (_isClosing)
            {
                _closeProgress += dt / 0.5f;

                if (_closeProgress >= 1f)
                {
                    _isClosing = false;
                    Active = false;

                    _particles.Clear();
                    _inPuzzleStub = false;
                    
                    ResearchUISystem.CloseCooldown = 15;
                    return;
                }
            }

            if (_warp.Busy || _isClosing) return;

            bool isBookActive = _bookAnim.State != BookAnimationManager.BookAnimState.Inactive;
            bool overlay = _popup.Visible || _warp.Busy || isBookActive;

            if (_inPuzzleStub && !_puzzleRenderer.IsStartupComplete()) overlay = true;

            bool overEmpty = !overlay && !_panel.Contains(mouse);

            if (_inPuzzleStub)
            {
                var form = ConstellationRegistry.TryGet(_warpReturnNodeId, out var entry) ? PuzzleRegistry.Get(entry.PuzzleShapeID) : null;
                var center = _cam.WorldToScreen(Vector2.Zero);
                var hoverNode = _puzzleRenderer.GetHoveredNode(form, center, _cam.Zoom, mouse);

                overEmpty = overEmpty && _selectedPuzzleNode == null && hoverNode == null;
            }
            else
            {
                overEmpty = overEmpty && HoveredNode(plr, mouse) == null;
            }

            if (!overlay) _cam.Update(overEmpty);
            else if (_inPuzzleStub && !_warp.Busy) _cam.Update(false);

            bool click = Main.mouseLeft && !_prevMouseLeft;

            if (click)
            {
                HandleClick(plr, mouse);
            }

            _prevMouseLeft = Main.mouseLeft;
            bool rightClick = Main.mouseRight && !_prevMouseRight;

            if (rightClick) HandleRightClick();
            _prevMouseRight = Main.mouseRight;
        }
        private void HandleRightClick()
        {
            if (_warp.Busy) return;

            if (_popup.Visible)
            {
                _popup.Hide();
                return;
            }

            if (_inPuzzleStub)
            {
                var plr = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                if (!plr.IsCompleted(_warpReturnNodeId))
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NodeExit") with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) });
                }
                BeginWarpReverse();
                return;
            }
            Close();
        }
        private void ApplyWarpCamera()
        {
            float ol = _warp.OffsetLerp();
            float zm = _warp.ZoomMul();

            if (_warp.State == WarpTransition.Phase.Forward)
                _cam.Offset = Vector2.Lerp(_warpStartOffset, _warpFocusWorld, ol);
            else
                _cam.Offset = _warpFocusWorld;
            _cam.Zoom = _warpStartZoom * zm;
        }
        private ConstellationEntry HoveredNode(ResearchPlayer player, Vector2 mouse)
        {
            foreach (var e in ConstellationRegistry.All.Values)
            {
                if (_nodes.IsHover(e, _cam, mouse)) return e;
            }
            return null;
        }
        private void HandleClick(ResearchPlayer plr, Vector2 mouse)
        {
            if (_popup.Visible)
            {
                _popup.CheckClick(mouse, out bool clickRes, out bool clickCan, out bool clickForce);

                if (clickRes && _popup.CanConfirm(plr))
                {
                    var target = _popup.Target;
                    plr.StartResearch(target.ID);
                    GraphState.RecomputeAll(plr);

                    _popup.Hide();
                    BeginWarpForward(target);

                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NodeExtension") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
                }
                else if (clickCan)
                {
                    _popup.Hide();
                }
                else if (clickForce)
                {
                    var target = _popup.Target;
                    if (!plr.IsCompleted(target.ID))
                    {
                        plr.CompleteResearch(target.ID);
                        GraphState.RecomputeAll(plr);

                        _bookAnim.Trigger(target.ID, _cam.WorldToScreen(target.WorldPosition));

                        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NodeExtension") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) });
                    }
                    _popup.Hide();
                }
                return;
            }

            if (_warp.Busy) return;

            if (_inPuzzleStub)
            {
                var center = _cam.WorldToScreen(Vector2.Zero);

                if (ConstellationRegistry.TryGet(_warpReturnNodeId, out var entry))
                {
                    var form = PuzzleRegistry.Get(entry.PuzzleShapeID);
                    if (form != null)
                    {
                        var states = PuzzleStateResolver.Evaluate(form, plr);
                        var hoverNode = _puzzleRenderer.GetHoveredNode(form, center, _cam.Zoom, mouse);
                        if (_selectedPuzzleNode != null)
                        {
                            if (_panel.HandleClick(mouse, plr, _selectedPuzzleNode, out bool paid, out EssenceType paidType, out int paidCost, out Vector2 startPos))
                            {
                                if (paid)
                                {
                                    int paidId = _selectedPuzzleNode.ID;
                                    _selectedPuzzleNode = null;
                                    _panel.SetHidden(true);
                                    _pendingPayments.Add(new PendingPayment { NodeID = paidId, Type = paidType });
                                    for (int i = 0; i < paidCost; i++)
                                    {
                                        _puzzleRenderer.SpawnFlyingEssence(paidId, paidType, startPos, delay: i * 0.15f);
                                    }
                                }
                                return;
                            }
                        }
                        if (hoverNode != null)
                        {
                            if (states.TryGetValue(hoverNode.ID, out var state) && state == PuzzleNodeState.Available)
                            {
                                _selectedPuzzleNode = hoverNode;
                                _panel.SetHidden(false);
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                return;
                            }
                        }
                        _selectedPuzzleNode = null;
                        _panel.SetHidden(true);
                    }
                }
                return;
            }
            
            if (_panel.HandleClick(mouse, plr, null, out _, out _, out _, out _)) return;

            var node = HoveredNode(plr, mouse);
            if (node == null) return;

            var st = GraphState.Get(node.ID);

            if (st == NodeState.Revealed || st == NodeState.Completed) _popup.Show(node, _cam.WorldToScreen(node.WorldPosition));
            else if (st == NodeState.Active) BeginWarpForward(node);
        }
        public void Draw(SpriteBatch sb, Player player)
        {
            if (!Active) return;

            var plr = player.GetModPlayer<ResearchPlayer>();
            int q = ResearchClientConfig.Instance?.QualityLevel ?? 2;

            if (!_inPuzzleStub)
            {
                _bg.Draw(sb, _cam, q);
                DrawEdges(sb, plr);

                var hover = (_warp.Busy || _popup.Visible) ? null : HoveredNode(plr, new Vector2(Main.mouseX, Main.mouseY));
                float dt = 1f / 60f;

                foreach (var e in ConstellationRegistry.All.Values)
                    _nodes.DrawNode(sb, e, _cam, _time, hover == e, dt);

                _particles.Draw(sb, Common.Draw.Pixel, _cam.WorldToScreen, _cam.Zoom);

                if (!_warp.Busy && !_popup.Visible) DrawTooltip(sb, plr, hover);
            }
            else DrawPuzzleStub(sb, plr);

            _panel.Draw(sb, plr, _time);
            DrawHeader(sb, plr);
            _popup.Draw(sb, plr, _time);
            _warp.DrawTunnel(sb);

            if (Jarkos_config.ScreenFlashesEnabled)
            {
                if (_isClosing)
                {
                    float flash = MathF.Pow(_closeProgress, 2f);
                    sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * flash);
                }
                else if (_openProgress < 1f)
                {
                    _openProgress = Math.Min(1f, _openProgress + 1f/60f / 0.8f);
                    float flash = MathF.Pow(1f - _openProgress, 3f);
                    sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * flash);
                }
            }
            else
            {
                if (_openProgress < 1f) _openProgress = 1f;
            }
            _bookAnim.Draw(sb);
        }
        private void DrawEdges(SpriteBatch sb, ResearchPlayer player)
        {
            foreach (var e in ConstellationRegistry.All.Values)
            {
                Vector2 cpos = _cam.WorldToScreen(e.WorldPosition);
                Color tier = EssencePalette.ColorOf(e.Tier);

                bool eHidden = GraphState.Get(e.ID) == NodeState.Hidden;

                foreach (var preId in e.Prerequisites)
                {
                    if (!ConstellationRegistry.TryGet(preId, out var p)) continue;
                    bool pHidden = GraphState.Get(preId) == NodeState.Hidden;
                    Vector2 ppos = _cam.WorldToScreen(p.WorldPosition);

                    if (eHidden || pHidden)
                    {
                        float hiddenCurve = MathF.Sin(_time * 0.8f + e.ID.GetHashCode() % 100 * 0.1f) * 6f * _cam.Zoom;
                        Common.Draw.GradientLine(sb, ppos, cpos, new Color(40, 50, 80) * 0.6f, 1f * _cam.Zoom, 1f, 16, hiddenCurve);
                    }
                    else
                    {
                        float timeOffset = _time * 1.5f + e.ID.GetHashCode() % 100 * 0.1f;
                        float curve = MathF.Sin(timeOffset) * 15f * _cam.Zoom;
                        float revealRaw = _nodes.EdgeReveal(e.ID);
                        float reveal = Math.Max(0f, revealRaw);

                        Common.Draw.GradientLine(sb, ppos, cpos, tier * 0.7f, 2f * _cam.Zoom, reveal, 24, curve);

                        if (revealRaw > 0f && revealRaw < 1f)
                        {
                            Vector2 tip = Common.Draw.GetCurvePoint(ppos, cpos, revealRaw, curve);
                            
                            Common.Draw.RadialGlow(sb, tip, 35f * _cam.Zoom, Color.White * 0.9f);
                            Common.Draw.RadialGlow(sb, tip, 70f * _cam.Zoom, tier * 0.7f);
                        }
                        if (reveal >= 1f && GraphState.Get(e.ID) == NodeState.Active && ResearchClientConfig.Instance?.Particles != false)
                        {
                            float rawT = (_time * 0.5f + Math.Abs(e.ID.GetHashCode()) * 0.001f) % 1f;
                            float smoothT = rawT * rawT * (3f - 2f * rawT);
                            float fade = MathF.Sin(rawT * MathF.PI);
                            for (int k = 0; k < 8; k++)
                            {
                                float tailT = MathF.Max(0f, smoothT - k * 0.012f);
                                Vector2 tailPos = Common.Draw.GetCurvePoint(ppos, cpos, tailT, curve);
                                float tailAlpha = (1f - (k / 8f)) * fade;
                                Common.Draw.RadialGlow(sb, tailPos, (6f - k * 0.4f) * _cam.Zoom, tier * 0.9f * tailAlpha);
                            }
                            Vector2 sparkPos = Common.Draw.GetCurvePoint(ppos, cpos, smoothT, curve);
                            sb.Draw(Common.Draw.Pixel, sparkPos, null, Color.White * fade, 0f, new Vector2(0.5f), new Vector2(2f * _cam.Zoom), SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }
        private void DrawTooltip(SpriteBatch sb, ResearchPlayer plr, ConstellationEntry hover)
        {
            if (hover == null) return;
            var st = GraphState.Get(hover.ID);

            string status = st switch
            {
                NodeState.Revealed => "Available — LMB to inspect", NodeState.Active => "Research in progress", NodeState.Completed => "Researched", NodeState.Hidden => "Requires previous nodes", _ => ""
            };

            string name = st == NodeState.Hidden ? "???" : hover.DisplayName;
            string txt = $"{name}\n{status}";

            var size = FontAssets.MouseText.Value.MeasureString(txt) * 0.85f;
            var pos = new Vector2(Main.mouseX + 20, Main.mouseY + 20);
            var box = new Rectangle((int)pos.X - 10, (int)pos.Y - 8, (int)size.X + 20, (int)size.Y + 16);

            Color boxCol = st == NodeState.Hidden ? new Color(40, 40, 50) : EssencePalette.ColorOf(hover.Tier);
            Panel.Rounded(sb, box, 9, new Color(12, 14, 30) * 0.96f, boxCol, 2);
            Terraria.Utils.DrawBorderString(sb, txt, pos, Color.White, 0.85f);
        }
        private void DrawPuzzleStub(SpriteBatch sb, ResearchPlayer plr)
        {
            sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(6, 7, 18));

            var center = _cam.WorldToScreen(Vector2.Zero);

            ConstellationRegistry.TryGet(_warpReturnNodeId, out var node);
            Color tier = node != null ? EssencePalette.ColorOf(node.Tier) : new Color(150, 180, 255);

            bool completed = plr.IsCompleted(_warpReturnNodeId);

            if (!completed && _puzzleTime < 0.8f)
            {
                float wave = _puzzleTime / 0.8f;
                Common.Draw.RadialGlow(sb, center, wave * 1800f, tier * (1f - wave) * 0.8f);
            }

            float compT = (completed && _puzzleTime >= 100f) ? _puzzleTime - 100f : 0f;

            if (compT > 0f && compT < 1.5f)
            {
                float build = compT / 1.5f;
                float buildEase = Easing.EaseInCubic(build);
                _cam.TargetZoom = 1.2f - buildEase * 0.2f;

                for(int i = 0; i < 8; i++)
                {
                    float ang = i * MathF.PI / 4f + _time * 2f;
                    float dist = 1200f * (1f - buildEase);
                    Vector2 p = center + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * dist;
                    Common.Draw.RadialGlow(sb, p, 150f * buildEase, tier * buildEase);
                }

                Common.Draw.RadialGlow(sb, center, 300f + buildEase * 800f, Color.White * buildEase * 0.7f);
            }

            float puzzleAlpha = 1f;
            if (compT > 0f)
            {
                puzzleAlpha = MathF.Max(0f, 1f - compT / 0.8f);
            }

            Common.Draw.RadialGlow(sb, center, 220f + 30f * Easing.Breathe(_time, 1.5f), tier * 0.12f * puzzleAlpha);
            float puzzleProgress = 0f;

            var form = node != null ? PuzzleRegistry.Get(node.PuzzleShapeID) : null;

            if (form != null)
            {
                var states = PuzzleStateResolver.Evaluate(form, plr);
                int totalCore = Enumerable.Count(form.Nodes, n => n.Type == PuzzleNodeType.Normal || n.Type == PuzzleNodeType.Final);
                int litCore = Enumerable.Count(form.Nodes, n => (n.Type == PuzzleNodeType.Normal || n.Type == PuzzleNodeType.Final) && states.TryGetValue(n.ID, out var st) && st == PuzzleNodeState.Lit);

                if (totalCore > 0) puzzleProgress = (float)litCore / totalCore;

                var hoverNode = completed || _warp.Busy || _popup.Visible ? null : _puzzleRenderer.GetHoveredNode(form, center, _cam.Zoom, new Vector2(Main.mouseX, Main.mouseY));
                _puzzleRenderer.Draw(sb, form, states, plr, center, _time, _cam.Zoom, _selectedPuzzleNode, hoverNode, puzzleAlpha);

                if (hoverNode != null && !completed)
                {
                    var nodeState = states.TryGetValue(hoverNode.ID, out var s) ? s : PuzzleNodeState.Locked;
                    string txt = "";
                    Color boxCol = new Color(40, 40, 50);

                    if (nodeState == PuzzleNodeState.Locked)
                    {
                        txt = "Required: ???";
                    }
                    else if (nodeState == PuzzleNodeState.Available)
                    {
                        string eName = EssencePalette.DisplayName(hoverNode.RequiredEssence);
                        int cost = EssencePanel.GetRequiredEssenceCost(node, hoverNode.RequiredEssence);

                        txt = $"Required: {eName} x{cost}";
                        boxCol = EssencePalette.ColorOf(hoverNode.RequiredEssence);
                    }
                    else if (nodeState == PuzzleNodeState.Lit)
                    {
                        txt = "";
                    }
                    if (!string.IsNullOrEmpty(txt))
                    {
                        var size = FontAssets.MouseText.Value.MeasureString(txt) * 0.85f;
                        var pos = new Vector2(Main.mouseX + 20, Main.mouseY + 20);
                        var box = new Rectangle((int)pos.X - 10, (int)pos.Y - 8, (int)size.X + 20, (int)size.Y + 16);

                        Panel.Rounded(sb, box, 9, new Color(12, 14, 30) * 0.96f, boxCol, 2);
                        Terraria.Utils.DrawBorderString(sb, txt, pos, Color.White, 0.85f);
                    }
                }
            }

            float landP = Easing.Clamp01(_puzzleTime / 0.8f);
            float textYOffset = (1f - MathSystem.EaseOutBack(landP)) * -100f;
            float textAlpha = MathSystem.EaseInCubic(Easing.Clamp01(landP * 2f));
            float btnYOffset = (1f - MathSystem.EaseOutCubic(landP)) * 60f;
            float btnAlpha = MathSystem.EaseInCubic(landP);

            if (compT > 0f)
            {
                float flyUp = MathSystem.EaseInOutCubic(MathF.Min(compT / 1.5f, 1f));
                textYOffset -= flyUp * 200f;
                textAlpha *= 1f - flyUp * 0.5f;

                float btnFade = MathF.Max(0, 1f - compT / 0.5f);
                btnAlpha *= btnFade;
                btnYOffset += (1f - btnFade) * 50f;
            }
            string title = node?.DisplayName ?? "CONSTELLATION PUZZLE";
            string rewardName = node?.RewardItemName ?? "Unknown Artifact";

            var ts = FontAssets.DeathText.Value.MeasureString(title);
            var hudCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            float titleScale = 0.55f + 0.015f * MathF.Sin(_time * 2f);

            Color titleColor = Color.Lerp(Color.White, tier, 0.2f + 0.2f * MathF.Sin(_time * 3f));
            Vector2 titlePos = new Vector2(hudCenter.X, hudCenter.Y - 360 + textYOffset + MathF.Sin(_time * 1.5f) * 5f);

            Common.Draw.RadialGlow(sb, titlePos, ts.X * 0.6f, tier * 0.4f * textAlpha);
            Terraria.Utils.DrawBorderStringFourWay(sb, FontAssets.DeathText.Value, title, titlePos.X, titlePos.Y, titleColor * textAlpha, Color.Black * textAlpha, ts / 2f, titleScale);

            if (!string.IsNullOrEmpty(rewardName))
            {
                char[] scrambled = new char[rewardName.Length];
                for (int i = 0; i < rewardName.Length; i++)
                {
                    if (rewardName[i] == ' ')
                    {
                        scrambled[i] = ' ';
                        continue;
                    }
                    var rng = new Random(rewardName[i].GetHashCode() + i * 37);
                    float charThreshold = (float)rng.NextDouble();
                    if (puzzleProgress >= 0.99f || puzzleProgress > charThreshold * 0.85f)
                    {
                        scrambled[i] = rewardName[i];
                    }
                    else
                    {
                        var frameRng = new Random((int)(_time * 15f) + i);
                        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                        scrambled[i] = chars[frameRng.Next(chars.Length)];
                    }
                }

                string rewardText = new string(scrambled);
                var rs = FontAssets.MouseText.Value.MeasureString(rewardText);
                float rewardScale = 1.0f + 0.02f * MathF.Sin(_time * 2.5f);

                Vector2 rewardPos = titlePos + new Vector2(0, ts.Y * titleScale / 2f + 16f);
                Color rewardColor = Color.Lerp(new Color(150, 160, 180), tier, puzzleProgress);

                Terraria.Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, rewardText, rewardPos.X, rewardPos.Y, rewardColor * textAlpha, Color.Black * textAlpha, rs / 2f, rewardScale);
            }
            if (compT >= 1.5f)
            {
                float blast = (compT - 1.5f) / 0.5f;
                float blastEaseOut = MathSystem.EaseOutCubic(MathF.Min(blast, 1f));
                Common.Draw.RadialGlow(sb, center, blastEaseOut * 4000f, tier * (1f - blastEaseOut) * 2f);
                if (Jarkos_config.ScreenFlashesEnabled)
                {
                    float whiteFade = Math.Min(blast, 1f);
                    sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * whiteFade);
                }
                if (compT > 2.0f && !_warp.Busy)
                {
                    _graphUpdateDelay = 2.0f;
                    BeginWarpReverse(Color.White);
                }
            }
        }
        private float _hintHoverFade;
        private void DrawHeader(SpriteBatch sb, ResearchPlayer plr)
        {
            if (_warp.Busy || _inPuzzleStub) return;
            string hint = "CONTROLS:\n" + " LMB (Node) — Inspect / Start\n" + " LMB (Bg) — Pan Camera\n" + " Mouse Wheel — Zoom\n" + " RMB — Go Back / Exit";

            if (plr.HasActive)
            {
                hint += "\n\n[Research Active]\nClick the yellow star to enter the puzzle.";
            }

            float scale = 0.65f;
            Vector2 size = FontAssets.MouseText.Value.MeasureString(hint) * scale;
            Vector2 pos = new Vector2(20, 20);
            Rectangle hitBox = new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)size.X + 20, (int)size.Y + 20);

            bool hover = hitBox.Contains(Main.mouseX, Main.mouseY);
            _hintHoverFade = MathHelper.Clamp(_hintHoverFade + (hover ? 0.08f : -0.05f), 0f, 1f);
            
            Color idleColor = new Color(70, 80, 100) * 0.4f;
            Color hoverColor = new Color(220, 230, 255);
            Color finalColor = Color.Lerp(idleColor, hoverColor, _hintHoverFade);

            Terraria.Utils.DrawBorderString(sb, hint, pos, finalColor, scale);
        }
        public class BookAnimationManager
        {
            public enum BookAnimState { Inactive, Entering, WaitingForNode, Closing, Exiting }
            public BookAnimState State { get; private set; } = BookAnimState.Inactive;
            private float _time;
            private float _frameTime;
            private int _frame;
            private Vector2 _pos;
            private float _startX;
            private float _targetX;
            private string _nodeId;
            private Vector2 _nodeStartPos;
            private float _nodeFlyProgress;
            private ConstellationEntry _nodeEntry;
            private ParticlePool _bookParticles = new();
            private float _rotation;
            private float _hoverTime;
            private const int FRAME_WIDTH = 66;
            private const int FRAME_HEIGHT = 70;
            public void Trigger(string nodeId, Vector2 startPos)
            {
                _nodeId = nodeId;
                _nodeStartPos = startPos;

                ConstellationRegistry.TryGet(nodeId, out _nodeEntry);
                State = BookAnimState.Entering;

                _time = 0f;
                _frame = 0;
                _frameTime = 0f;
                _startX = -300f;
                _targetX = 150f;
                _pos = new Vector2(_startX, Main.screenHeight / 2f);
                _nodeFlyProgress = 0f;
                _rotation = 0f;
                _hoverTime = 0f;
            }
            public void Update(float dt)
            {
                if (State == BookAnimState.Inactive) return;

                _time += dt;
                _hoverTime += dt;
                _bookParticles.Update(dt);

                float hoverY = MathF.Sin(_hoverTime * 2f) * 10f;
                Vector2 drawPos = new Vector2(_pos.X, (Main.screenHeight / 2f) + hoverY);

                if (Main.rand.NextBool(2))
                {
                    Vector2 offset = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-20f, 20f));
                    Vector2 pPos = drawPos + offset;
                    Vector2 pVel = Vector2.Normalize(offset) * Main.rand.NextFloat(2f, 4f);

                    _bookParticles.Emit(pPos, pVel, Color.DeepPink, 0.7f, Main.rand.NextFloat(2f, 4f), isWorld: false);
                }
                if (State == BookAnimState.Entering)
                {
                    float t = Math.Min(1f, _time / 0.8f);
                    float ease = 1f - MathF.Pow(1f - t, 5f);

                    _pos.X = MathHelper.Lerp(_startX, _targetX, ease);
                    _rotation = MathHelper.Lerp(0.4f, 0f, ease) + (float)Math.Sin(_time * 4f) * 0.05f;
                    _frameTime += dt;

                    if (_frameTime > 0.12f)
                    {
                        _frameTime = 0f;
                        if (_frame < 5) _frame++;
                    }
                    if (t >= 1f && _frame == 5)
                    {
                        State = BookAnimState.WaitingForNode;
                        _time = 0f;
                    }
                }
                else if (State == BookAnimState.WaitingForNode)
                {
                    _frame = 5;
                    _time += dt;
                    _rotation = (float)Math.Sin(_time * 2f) * 0.08f;
                    _nodeFlyProgress += dt / 0.6f;

                    float flyT = Math.Min(1f, _nodeFlyProgress);

                    if (flyT >= 1f)
                    {
                        State = BookAnimState.Closing;
                        _time = 0f;
                        _frameTime = 0f;
                        _rotation = 0f;
                        SoundEngine.PlaySound(SoundID.MenuTick);

                        Color essenceColor = EssencePalette.ColorOf(_nodeEntry.Tier);

                        _bookParticles.Burst(_pos, essenceColor, count: 40, speed: 180f, life: 0.8f, isWorld: false);
                    }
                }
                else if (State == BookAnimState.Closing)
                {
                    _frameTime += dt;
                    if (_frameTime > 0.12f)
                    {
                        if (_frame < 10)
                        {
                            _frameTime = 0f;
                            _frame++;
                        }
                    }
                    if (_frame == 10 && _frameTime > 0.15f)
                    {
                        State = BookAnimState.Exiting;
                        _time = 0f;
                        _startX = _pos.X;
                        _targetX = -300f;
                    }
                }
                else if (State == BookAnimState.Exiting)
                {
                    float t = Math.Min(1f, _time / 0.7f);
                    float ease = t * t * t;
                    _pos.X = MathHelper.Lerp(_startX, _targetX, ease);
                    _rotation = MathHelper.Lerp(0f, -0.5f, ease);
                    if (t >= 1f)
                    {
                        State = BookAnimState.Inactive;
                    }
                }
            }
            public void Draw(SpriteBatch sb)
            {
                if (State == BookAnimState.Inactive) return;

                Texture2D bookTex = ModContent.Request<Texture2D>("jarkos/UI/ResearchBookIcon").Value;
                float hoverY = MathF.Sin(_hoverTime * 2f) * 10f;

                Vector2 drawPos = new Vector2(_pos.X, (Main.screenHeight / 2f) + hoverY);

                _bookParticles.Draw(sb, Common.Draw.Pixel, (v) => v, 1f);
                Rectangle sourceRect = new Rectangle(0, _frame * FRAME_HEIGHT, FRAME_WIDTH, FRAME_HEIGHT);

                sb.Draw(bookTex, drawPos, sourceRect, Color.White, _rotation, new Vector2(FRAME_WIDTH / 2f, FRAME_HEIGHT / 2f), 1f, SpriteEffects.None, 0f);

                if (State == BookAnimState.WaitingForNode && _nodeEntry != null)
                {
                    float flyT = Math.Min(1f, _nodeFlyProgress);
                    float easeFly = flyT * flyT * (3f - 2f * flyT);

                    Vector2 currentScreen = Vector2.Lerp(_nodeStartPos, drawPos, easeFly);
                    Vector2 dir = drawPos - _nodeStartPos;

                    float dist = dir.Length();
                    if (dist > 0)
                    {
                        Vector2 normal = new Vector2(-dir.Y, dir.X) / dist;
                        float arcHeight = dist * 0.3f;
                        currentScreen += normal * MathF.Sin(easeFly * MathF.PI) * arcHeight;
                    }

                    Texture2D tex = ModContent.Request<Texture2D>(_nodeEntry.IconTexture).Value;
                    Color tint = EssencePalette.ColorOf(_nodeEntry.Tier);

                    for (int i = 0; i < 5; i++)
                    {
                        float trailT = Math.Max(0f, flyT - i * 0.05f);
                        float easeTrail = trailT * trailT * (3f - 2f * trailT);
                        Vector2 trailPos = Vector2.Lerp(_nodeStartPos, drawPos, easeTrail);

                        if (dist > 0)
                        {
                            Vector2 normal = new Vector2(-dir.Y, dir.X) / dist;
                            float arcHeight = dist * 0.3f;
                            trailPos += normal * MathF.Sin(easeTrail * MathF.PI) * arcHeight;
                        }
                        sb.Draw(tex, trailPos, null, tint * (0.5f - i * 0.1f), 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    }
                    sb.Draw(tex, currentScreen, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
