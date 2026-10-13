using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using jarkos.systems;
using Terraria.Localization;

namespace jarkos.UI
{
    public class GoldenTrophyUI
    {
        public bool IsOpen { get; private set; }
        public bool IsClosing { get; private set; }
        public float OpenProgress { get; private set; } = 0f;
        public int CurrentTab { get; set; } = 0;

        public float GraphZoom { get; set; } = 1.0f;
        public float GraphScrollOffset { get; set; } = 0.0f;

        private bool _isDraggingZoom = false;
        private bool _isDraggingScroll = false;

        private Point? _tileCoords;

        public void Open(Point? tileCoords = null)
        {
            IsOpen = true;
            IsClosing = false;
            OpenProgress = 0f;
            GraphZoom = 1.0f;
            GraphScrollOffset = 0.0f;

            _isDraggingZoom = false;
            _isDraggingScroll = false;
            _tileCoords = tileCoords;

            Main.playerInventory = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public void Close()
        {
            if (!IsOpen || IsClosing) return;
            IsClosing = true;

            _isDraggingZoom = false;
            _isDraggingScroll = false;

            Main.playerInventory = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        public void Update(GameTime gameTime)
        {
            if (!IsOpen) return;
            Player player = Main.LocalPlayer;

            if (IsClosing)
            {
                OpenProgress -= (float)gameTime.ElapsedGameTime.TotalSeconds / 0.4f;
                
                if (OpenProgress <= 0f)
                {
                    OpenProgress = 0f;
                    IsClosing = false;
                    IsOpen = false;
                    return;
                }
            }
            else if (OpenProgress < 1f)
            {
                OpenProgress = Math.Min(1f, OpenProgress + (float)gameTime.ElapsedGameTime.TotalSeconds / 0.5f);
            }

            player.mouseInterface = true;

            if (!IsClosing)
            {
                if (player.dead || !player.active || !Main.playerInventory || PlayerInput.Triggers.JustPressed.Inventory || PlayerInput.Triggers.JustPressed.OpenCreativePowersMenu || Main.mapFullscreen) 
                {
                    Close();
                }
                else if (_tileCoords.HasValue)
                {
                    Vector2 tileWorldPos = new Vector2(_tileCoords.Value.X * 16f + 16f, _tileCoords.Value.Y * 16f + 24f);

                    if (Vector2.Distance(player.Center, tileWorldPos) > 192f)
                    {
                        Close();
                    }
                }
            }

            if (!Main.mouseLeft)
            {
                _isDraggingZoom = false;
                _isDraggingScroll = false;
            }
        }

        private Vector2 GetUIBasePos()
        {
            float ease = MathSystem.EaseOutCubic(OpenProgress);
            float offsetY = (1f - ease) * 450f;
            return new Vector2(Main.screenWidth / 2f - 300f, Main.screenHeight / 2f - 220f + offsetY);
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsOpen) return;

            float uiAlpha = MathSystem.EaseOutCubic(OpenProgress);
            Vector2 basePos = GetUIBasePos();

            DrawHeaderAndTabs(sb, basePos, uiAlpha);

            BossFightRecord fight = BossStatsTrackerSystem.LastCompletedFight;

            if (CurrentTab == 0)
            {
                DrawOverviewPage(sb, basePos, fight, uiAlpha);
            }
            else
            {
                DrawGraphPage(sb, basePos, fight, uiAlpha);
            }
        }

        private void DrawHeaderAndTabs(SpriteBatch sb, Vector2 basePos, float alpha)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/UI/GoldenTrophyPanelGlow").Value;
            Vector2 glowOrigin = new Vector2(glowTex.Width / 2f, 0f);
            Vector2 glowPos = new Vector2(basePos.X + 300f, basePos.Y + 8f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            sb.Draw(glowTex, glowPos, null, Color.White * (0.85f * alpha), 0f, glowOrigin, 1f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Texture2D panelTex = ModContent.Request<Texture2D>("jarkos/UI/GoldenTrophyPanel").Value;
            Vector2 panelPos = new Vector2(basePos.X + 300f - panelTex.Width / 2f, basePos.Y + 8f);

            sb.Draw(panelTex, panelPos, Color.White * alpha);

            string title = (string)GTLanguageSystem.GTGolden_trophy;
            Vector2 titlePos = new Vector2(panelPos.X + 20f, panelPos.Y + 8f);

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, title, titlePos, Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.85f));

            string[] tabs = new string[] { (string)GTLanguageSystem.GTReview, (string)GTLanguageSystem.GTGraph };
            
            for (int i = 0; i < 2; i++)
            {
                Rectangle tabRect = new Rectangle((int)(panelPos.X + 175 + (i * 76)), (int)(panelPos.Y + 5), 72, 26);
                bool hovered = tabRect.Contains(Main.MouseScreen.ToPoint());
                bool active = CurrentTab == i;

                Color tabBgColor = active ? new Color(200, 160, 0, 220) : (hovered ? new Color(240, 200, 20, 240) : new Color(80, 65, 10, 160));
                Terraria.Utils.DrawInvBG(sb, tabRect, tabBgColor * alpha);

                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(tabs[i]) * 0.75f;
                Vector2 textPos = tabRect.Center.ToVector2() - textSize / 2f;
                Color tabTextColor = active ? Color.Gold * alpha : (hovered ? Color.Yellow * alpha : Color.White * (0.8f * alpha));
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, tabs[i], textPos, tabTextColor, 0f, Vector2.Zero, new Vector2(0.75f));

                if (hovered && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    CurrentTab = i;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
        }

        private void DrawOverviewPage(SpriteBatch sb, Vector2 basePos, BossFightRecord fight, float alpha)
        {
            if (fight == null || fight.PlayerStats == null || fight.PlayerStats.Count == 0)
            {
                string msg1 = (string)GTLanguageSystem.GTmsg1;
                string msg2 = (string)GTLanguageSystem.GTmsg2;
                Vector2 s1 = FontAssets.MouseText.Value.MeasureString(msg1);
                Vector2 s2 = FontAssets.MouseText.Value.MeasureString(msg2) * 0.85f;

                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, msg1, new Vector2(basePos.X + 300f - s1.X / 2f, basePos.Y + 210f), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, msg2, new Vector2(basePos.X + 300f - s2.X / 2f, basePos.Y + 245f), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.85f));
                return;
            }

            int dur = Math.Max(1, fight.DurationSeconds);
            int min = dur / 60;
            int sec = dur % 60;

            string bossDisplayName = (fight.BossType > 0) ? Lang.GetNPCNameValue(fight.BossType) : fight.BossName;
            if (string.IsNullOrEmpty(bossDisplayName)) bossDisplayName = fight.BossName;

            string minStr = (string)GTLanguageSystem.GTmin;
            string secStr = (string)GTLanguageSystem.GTsec;
            string fightInfo = (string)GTLanguageSystem.GTboss + " " + $"{bossDisplayName}" + (string)GTLanguageSystem.GTtimebattle + " " + $"{min} {minStr} {sec:D2} {secStr}";

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, fightInfo, new Vector2(basePos.X + 30f, basePos.Y + 65f), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.9f));

            Texture2D givenIcon = ModContent.Request<Texture2D>("jarkos/UI/GivenDamageIcon").Value;
            Texture2D receivedIcon = ModContent.Request<Texture2D>("jarkos/UI/ReceivedDamageIcon").Value;
            Texture2D blockedIcon = ModContent.Request<Texture2D>("jarkos/UI/BlockedDamageIcon").Value;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            int yOffset = 95;
            foreach (var kvp in fight.PlayerStats)
            {
                PlayerFightStats stats = kvp.Value;
                if (stats == null) continue;

                Rectangle card = new Rectangle((int)basePos.X + 30, (int)basePos.Y + yOffset, 540, 70);

                Rectangle colorBadge = new Rectangle(card.X + 12, card.Y + 12, 16, 16);
                sb.Draw(pixel, colorBadge, new Rectangle(0, 0, 1, 1), stats.ChartColor * alpha);

                string defaultPlayerName = (string)GTLanguageSystem.GTplayer;
                string pName = (string.IsNullOrEmpty(stats.PlayerName) || stats.PlayerName == "Player") ? defaultPlayerName : stats.PlayerName;
                string playerText = (string)GTLanguageSystem.GTplayer + " " + $"{pName}";

                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, playerText, new Vector2(card.X + 36, card.Y + 10), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.95f));

                string dmgDealt = (string)GTLanguageSystem.GTdamage + $"{stats.TotalDamageDealt:N0}";
                string dmgTaken = (string)GTLanguageSystem.GTtaked + $"{stats.TotalDamageTaken:N0}";
                string dmgBlocked = (string)GTLanguageSystem.GTblocked + $"{stats.TotalDamageBlocked:N0}";

                Vector2 posDealt = new Vector2(card.X + 35, card.Y + 38);
                sb.Draw(givenIcon, new Rectangle((int)posDealt.X - 22, (int)posDealt.Y + 1, 18, 18), Color.White * alpha);
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, dmgDealt, posDealt, Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.8f));

                Vector2 posTaken = new Vector2(card.X + 215, card.Y + 38);
                sb.Draw(receivedIcon, new Rectangle((int)posTaken.X - 22, (int)posTaken.Y + 1, 18, 18), Color.White * alpha);
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, dmgTaken, posTaken, Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.8f));

                Vector2 posBlocked = new Vector2(card.X + 395, card.Y + 38);
                sb.Draw(blockedIcon, new Rectangle((int)posBlocked.X - 22, (int)posBlocked.Y + 1, 18, 18), Color.White * alpha);
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, dmgBlocked, posBlocked, Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.8f));

                yOffset += 78;
                if (yOffset > 350) break;
            }
        }

        private void DrawGraphPage(SpriteBatch sb, Vector2 basePos, BossFightRecord fight, float alpha)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            if (fight == null || fight.PlayerStats == null || fight.PlayerStats.Count == 0)
            {
                DrawOverviewPage(sb, basePos, fight, alpha);
                return;
            }

            Rectangle graphBox = new Rectangle((int)basePos.X + 50, (int)basePos.Y + 80, 500, 260);

            Point mousePt = Main.MouseScreen.ToPoint();
            if (graphBox.Contains(mousePt))
            {
                int scroll = PlayerInput.ScrollWheelDelta; 
                if (scroll > 0)
                {
                    GraphZoom = Math.Min(5.0f, GraphZoom + 0.25f);
                }
                else if (scroll < 0)
                {
                    GraphZoom = Math.Max(1.0f, GraphZoom - 0.25f);
                }
            }

            if (GraphZoom <= 1.01f)
            {
                GraphZoom = 1.0f;
                GraphScrollOffset = 0.0f;
            }

            float maxDmg = 100f;
            int maxTimelinePoints = 1;

            foreach (var kvp in fight.PlayerStats)
            {
                PlayerFightStats stats = kvp.Value;
                if (stats?.DamageTimeline == null) continue;

                if (stats.DamageTimeline.Count > maxTimelinePoints)
                {
                    maxTimelinePoints = stats.DamageTimeline.Count; 
                }
                foreach (float val in stats.DamageTimeline)
                {
                    if (val > maxDmg) maxDmg = val;
                }
            }

            int duration = Math.Max(1, maxTimelinePoints);
            float totalGraphWidth = graphBox.Width * GraphZoom;
            float maxScrollPixels = Math.Max(0f, totalGraphWidth - graphBox.Width);
            float currentScrollX = GraphScrollOffset * maxScrollPixels;

            string dpsTitle = "DPS";
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, dpsTitle, new Vector2(basePos.X + 380, basePos.Y + 58), Color.Yellow * (0.8f * alpha), 0f, Vector2.Zero, new Vector2(0.65f));

            Color gridColor = Color.White * (0.25f * alpha);
            for (int i = 1; i <= 4; i++)
            {
                int y = graphBox.Bottom - (int)((graphBox.Height - 12) * (i / 4f)) - 6;
                sb.Draw(pixel, new Rectangle(graphBox.X, y, graphBox.Width, 1), new Rectangle(0, 0, 1, 1), gridColor);

                float yVal = maxDmg * (i / 4f);
                string label = yVal >= 1000f ? $"{yVal / 1000f:F1}k" : $"{yVal:F0}";
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, label, new Vector2(graphBox.X - 38, y - 8), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.65f));
            }

            int gridStepSeconds = Math.Max(1, (int)(duration / (5f * GraphZoom)));
            for (int sec = 0; sec <= duration; sec += gridStepSeconds)
            {
                float normX = (float)sec / duration;
                float px = graphBox.X + (normX * totalGraphWidth) - currentScrollX;

                if (px >= graphBox.X && px <= graphBox.Right)
                {
                    sb.Draw(pixel, new Rectangle((int)px, graphBox.Y, 1, graphBox.Height), new Rectangle(0, 0, 1, 1), gridColor);
                    string label = $"{sec}" + " " + (string)GTLanguageSystem.GTsec_ongraph;
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, label, new Vector2(px - 10, graphBox.Bottom + 4), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.65f));
                }
            }

            int legX = graphBox.X;
            foreach (var kvp in fight.PlayerStats)
            {
                PlayerFightStats stats = kvp.Value;
                if (stats == null) continue;
                Rectangle legBadge = new Rectangle(legX, graphBox.Y - 20, 12, 12);
                sb.Draw(pixel, legBadge, new Rectangle(0, 0, 1, 1), stats.ChartColor * alpha);

                string defaultPlayerName = (string)GTLanguageSystem.GTplayer;
                string pName = (string.IsNullOrEmpty(stats.PlayerName) || stats.PlayerName == "Player") ? defaultPlayerName : stats.PlayerName;
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, pName, new Vector2(legX + 16, graphBox.Y - 22), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.7f));
                legX += (int)(FontAssets.MouseText.Value.MeasureString(pName).X * 0.7f) + 30;
            }

            Vector2? hoveredPoint = null;
            string hoveredTooltip = "";

            foreach (var kvp in fight.PlayerStats)
            {
                PlayerFightStats stats = kvp.Value;
                if (stats?.DamageTimeline == null || stats.DamageTimeline.Count == 0) continue;

                List<float> timeline = stats.DamageTimeline;
                int totalPts = timeline.Count;

                Vector2? prevPoint = null;
                for (int ptIdx = 0; ptIdx <= totalPts; ptIdx++)
                {
                    float secDmg = (ptIdx == 0) ? 0f : timeline[ptIdx - 1];

                    float normX = (float)ptIdx / totalPts;
                    float normY = MathHelper.Clamp(secDmg / maxDmg, 0f, 1f);

                    float px = graphBox.X + (normX * totalGraphWidth) - currentScrollX;
                    float py = graphBox.Bottom - 6 - (normY * (graphBox.Height - 12));
                    Vector2 currPoint = new Vector2(px, py);

                    if (prevPoint.HasValue)
                    {
                        DrawClippedLine(sb, prevPoint.Value, currPoint, graphBox, stats.ChartColor, 3f, alpha);
                    }

                    if (px >= graphBox.X - 2 && px <= graphBox.Right + 2 && py >= graphBox.Y - 2 && py <= graphBox.Bottom + 2)
                    {
                        Rectangle dotRect = new Rectangle((int)px - 3, (int)py - 3, 6, 6);
                        sb.Draw(pixel, dotRect, new Rectangle(0, 0, 1, 1), stats.ChartColor * alpha);

                        if (Vector2.Distance(Main.MouseScreen, currPoint) < 10f)
                        {
                            hoveredPoint = currPoint;
                            string defaultPlayerName = (string)GTLanguageSystem.GTplayer;
                            string pName = (string.IsNullOrEmpty(stats.PlayerName) || stats.PlayerName == "Player") ? defaultPlayerName : stats.PlayerName;

                            hoveredTooltip = (string)GTLanguageSystem.GTsec_ongraph + $"{pName}\n {ptIdx}s\n {secDmg:N0}";
                        }
                    }

                    prevPoint = currPoint;
                }
            }

            DrawSliders(sb, basePos, alpha);

            if (hoveredPoint.HasValue && !string.IsNullOrEmpty(hoveredTooltip))
            {
                sb.Draw(pixel, new Rectangle((int)hoveredPoint.Value.X - 5, (int)hoveredPoint.Value.Y - 5, 10, 10), new Rectangle(0, 0, 1, 1), Color.Yellow * alpha);

                Vector2 mousePos = Main.MouseScreen + new Vector2(15, 15);
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, hoveredTooltip, new Vector2(mousePos.X + 8, mousePos.Y + 6), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.75f));
            }
        }

        private void DrawSliders(SpriteBatch sb, Vector2 basePos, float alpha)
        {
            Point mousePt = Main.MouseScreen.ToPoint();

            string zoomLabel = (string)GTLanguageSystem.GTscale + $"{GraphZoom:F1}x";
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, zoomLabel, new Vector2(basePos.X + 40, basePos.Y + 365), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.75f));

            Rectangle zoomTrack = new Rectangle((int)basePos.X + 40, (int)basePos.Y + 386, 220, 8);
            Terraria.Utils.DrawInvBG(sb, zoomTrack, new Color(40, 35, 20, 200) * alpha);

            float zoomNorm = (GraphZoom - 1.0f) / 4.0f;
            int handleX = (int)(zoomTrack.X + zoomNorm * (zoomTrack.Width - 16));
            Rectangle zoomHandle = new Rectangle(handleX, zoomTrack.Y - 5, 16, 18);

            bool hoverZoom = zoomTrack.Contains(mousePt) || zoomHandle.Contains(mousePt) || _isDraggingZoom;
            Color handleColor = hoverZoom ? new Color(255, 230, 40, 255) : new Color(210, 170, 10, 230);
            Terraria.Utils.DrawInvBG(sb, zoomHandle, handleColor * alpha);

            if (Main.mouseLeft && hoverZoom && !_isDraggingScroll)
            {
                _isDraggingZoom = true;
            }

            if (_isDraggingZoom)
            {
                float newNorm = MathHelper.Clamp((float)(mousePt.X - zoomTrack.X) / zoomTrack.Width, 0f, 1f);
                GraphZoom = 1.0f + newNorm * 4.0f;
            }

            if (GraphZoom > 1.05f)
            {
                string scrollLabel = (string)GTLanguageSystem.GTscroll;
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, scrollLabel, new Vector2(basePos.X + 320, basePos.Y + 365), Color.White * alpha, 0f, Vector2.Zero, new Vector2(0.75f));

                Rectangle scrollTrack = new Rectangle((int)basePos.X + 320, (int)basePos.Y + 386, 230, 8);
                Terraria.Utils.DrawInvBG(sb, scrollTrack, new Color(40, 35, 20, 200) * alpha);

                float visibleRatio = 1.0f / GraphZoom;
                int handleWidth = Math.Max(28, (int)(scrollTrack.Width * visibleRatio));
                int scrollHandleX = (int)(scrollTrack.X + GraphScrollOffset * (scrollTrack.Width - handleWidth));
                Rectangle scrollHandle = new Rectangle(scrollHandleX, scrollTrack.Y - 5, handleWidth, 18);

                bool hoverScroll = scrollTrack.Contains(mousePt) || scrollHandle.Contains(mousePt) || _isDraggingScroll;
                Color scrollHandleColor = hoverScroll ? new Color(255, 230, 40, 255) : new Color(210, 170, 10, 230);
                Terraria.Utils.DrawInvBG(sb, scrollHandle, scrollHandleColor * alpha);

                if (Main.mouseLeft && hoverScroll && !_isDraggingZoom)
                {
                    _isDraggingScroll = true;
                }

                if (_isDraggingScroll)
                {
                    float newScrollNorm = MathHelper.Clamp((float)(mousePt.X - scrollTrack.X - handleWidth / 2f) / (scrollTrack.Width - handleWidth), 0f, 1f);
                    GraphScrollOffset = newScrollNorm;
                }
            }
        }

        private static void DrawClippedLine(SpriteBatch sb, Vector2 start, Vector2 end, Rectangle bounds, Color color, float width, float alpha)
        {
            if ((start.X < bounds.X && end.X < bounds.X) || (start.X > bounds.Right && end.X > bounds.Right)) return;

            if (start.X != end.X)
            {
                if (start.X < bounds.X)
                {
                    float t = (bounds.X - start.X) / (end.X - start.X);
                    start = new Vector2(bounds.X, MathHelper.Lerp(start.Y, end.Y, t));
                }
                else if (start.X > bounds.Right)
                {
                    float t = (bounds.Right - start.X) / (end.X - start.X);
                    start = new Vector2(bounds.Right, MathHelper.Lerp(start.Y, end.Y, t));
                }

                if (end.X < bounds.X)
                {
                    float t = (bounds.X - end.X) / (start.X - end.X);
                    end = new Vector2(bounds.X, MathHelper.Lerp(end.Y, start.Y, t));
                }
                else if (end.X > bounds.Right)
                {
                    float t = (bounds.Right - end.X) / (start.X - end.X);
                    end = new Vector2(bounds.Right, MathHelper.Lerp(end.Y, start.Y, t));
                }
            }

            DrawLine(sb, start, end, color, width, alpha);
        }

        private static void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float width, float alpha)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 edge = end - start;
            float angle = MathF.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color * alpha, angle, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
        }
    }

    public class GTLanguageSystem : ModSystem
	{
		public static LocalizedText GTGolden_trophy { get; private set; }
		public static LocalizedText GTReview { get; private set; }
        public static LocalizedText GTGraph { get; private set; }
        public static LocalizedText GTmsg1 { get; private set; }
        public static LocalizedText GTmsg2 { get; private set; }
		public static LocalizedText GTsec { get; private set; }
        public static LocalizedText GTsec_ongraph { get; private set; }
		public static LocalizedText GTmin { get; private set; }
		public static LocalizedText GTtimebattle { get; private set; }
		public static LocalizedText GTplayer { get; private set; }
        public static LocalizedText GTboss { get; private set; }
        public static LocalizedText GTdamage { get; private set; }
		public static LocalizedText GTblocked { get; private set; }
		public static LocalizedText GTtaked { get; private set; }
        public static LocalizedText GTscale { get; private set; }
		public static LocalizedText GTscroll { get; private set; }

        public override void Load()
        {
            string category = "UI";
			GTGolden_trophy ??= Mod.GetLocalization($"{category}.GTGolden_trophy");
			GTReview ??= Mod.GetLocalization($"{category}.GTReview");
			GTGraph ??= Mod.GetLocalization($"{category}.GTGraph");
			GTmsg1 ??= Mod.GetLocalization($"{category}.GTmsg1");
			GTmsg2 ??= Mod.GetLocalization($"{category}.GTmsg2");
			GTsec ??= Mod.GetLocalization($"{category}.GTsec");
            GTsec_ongraph ??= Mod.GetLocalization($"{category}.GTsec_ongraph");
			GTmin ??= Mod.GetLocalization($"{category}.GTmin");
			GTtimebattle ??= Mod.GetLocalization($"{category}.GTtimebattle");
			GTplayer ??= Mod.GetLocalization($"{category}.GTplayer");
            GTboss ??= Mod.GetLocalization($"{category}.GTboss");
            GTdamage ??= Mod.GetLocalization($"{category}.GTsecGTdamage_ongraph");
			GTblocked ??= Mod.GetLocalization($"{category}.GTblocked");
			GTtaked ??= Mod.GetLocalization($"{category}.GTtaked");
			GTscale ??= Mod.GetLocalization($"{category}.GTscale");
            GTscroll ??= Mod.GetLocalization($"{category}.GTscroll");
        }
	}
}
