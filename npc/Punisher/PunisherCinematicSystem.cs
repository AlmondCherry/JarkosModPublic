using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace jarkos.npc.Punisher
{
    public class PunisherCinematicSystem : ModSystem
    {
        public static float VignetteIntensity = 0f;
        public static float PurpleFogOpacity = 0f;
        public static float UniverseTearOpacity = 0f;
        public static float PainOpacity = 0f;
        public static float WhiteFlashOpacity = 0f;
        public static float Phase2FogOpacity = 0f;
        public static float SpaceDistortionOpacity = 0f;
        public static float Phase2SlashOpacity = 0f;
        public static float KaleidoscopeOpacity = 0f;
        public static float WorldBlackoutOpacity = 0f;
        public static float HexBarrierOpacity = 0f;
        public static Vector2 HexBarrierCenter = Vector2.Zero;
        public static Vector2 HexBarrierSize = new Vector2(2500f, 1500f);
        public static int BlackoutTimer = 0;
        public static Vector2 BossWorldPos = Vector2.Zero;
        public static Vector2 TearPoint = new Vector2(0.5f, 0.5f);
        public static Vector2 TearNormal = Vector2.UnitX;
        public static Vector2 DistortionPoint = new Vector2(0.5f, 0.5f);
        public static Vector2 DistortionNormal = Vector2.UnitX;
        public static Vector2 BossScreenPos = new Vector2(0.5f, 0.5f);
        public override void UpdateUI(GameTime gameTime)
        {
            int npcIndex = NPC.FindFirstNPC(ModContent.NPCType<Punisher>());
            if (npcIndex != -1)
            {
                BossWorldPos = Main.npc[npcIndex].Center;
            }
            else if (Main.LocalPlayer.active)
            {
                BossWorldPos = Main.LocalPlayer.Center;
            }
            if (Main.screenWidth > 0 && Main.screenHeight > 0)
            {
                Vector2 screenPos = BossWorldPos - Main.screenPosition;
                BossScreenPos = new Vector2(screenPos.X / Main.screenWidth, screenPos.Y / Main.screenHeight);
            }
        }
        public class RealityShard
        {
            public Rectangle SourceRect;
            public Vector2 DrawPosition;
            public Vector2 Velocity;
            public float Rotation;
            public float RotVelocity;
            public float Parallax;
        }
        public static List<RealityShard> WorldShards = new List<RealityShard>();
        public static RenderTarget2D CapturedScreen;
        public static bool CaptureNextFrame = false;
        public static int BossAbsenceTimer = 0;
        public static Vector2 LastScreenPosition = Vector2.Zero;
        public static float ScreenShakeAmount = 0f;
        public override void ModifyScreenPosition()
        {
            if (ScreenShakeAmount > 0f)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(ScreenShakeAmount, ScreenShakeAmount);
                ScreenShakeAmount *= 0.9f;
                if (ScreenShakeAmount < 0.1f) ScreenShakeAmount = 0f;
            }
        }
        public override void PostDrawTiles()
        {
            if (CaptureNextFrame && Main.spriteBatch != null)
            {
                CaptureNextFrame = false;
                try
                {
                    if (CapturedScreen == null || CapturedScreen.Width != Main.screenWidth || CapturedScreen.Height != Main.screenHeight)
                    {
                        CapturedScreen = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                    }
                    var oldTargets = Main.graphics.GraphicsDevice.GetRenderTargets();

                    Main.graphics.GraphicsDevice.SetRenderTarget(CapturedScreen);
                    Main.graphics.GraphicsDevice.Clear(Color.Transparent);

                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                    Main.spriteBatch.End();

                    if (oldTargets != null && oldTargets.Length > 0)
                        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
                    else
                        Main.graphics.GraphicsDevice.SetRenderTarget(null);
                    WorldShards.Clear();
                    
                    int rows = 8;
                    int cols = 12;
                    int w = Main.screenWidth / cols;
                    int h = Main.screenHeight / rows;

                    for (int x = 0; x < cols; x++)
                    {
                        for (int y = 0; y < rows; y++)
                        {
                            Rectangle rect = new Rectangle(x * w, y * h, w, h);
                            Vector2 center = new Vector2(x * w + w/2, y * h + h/2);
                            Vector2 screenCenter = new Vector2(Main.screenWidth/2, Main.screenHeight/2);
                            Vector2 vel = (center - screenCenter) * 0.005f;

                            vel += Main.rand.NextVector2Circular(0.5f, 0.5f);
                            WorldShards.Add(new RealityShard()
                            {
                                SourceRect = new Rectangle(x * w, y * h, w, h), DrawPosition = new Vector2(x * w + w / 2f, y * h + h / 2f), Velocity = vel, Rotation = 0f, RotVelocity = Main.rand.NextFloat(-0.02f, 0.02f), Parallax = Main.rand.NextFloat(0.5f, 1.2f)
                            });
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (e.InnerException != null) Main.NewText("inner" + e.InnerException.Message, Color.Red);
                }
            }
        }
        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server) return;
                PainOpacity = 0f;
            if (VignetteIntensity > 0f) VignetteIntensity -= 0.01f;
            if (PurpleFogOpacity > 0f) PurpleFogOpacity -= 0.02f;
            if (UniverseTearOpacity > 0f) UniverseTearOpacity -= 0.05f;
            if (Phase2FogOpacity > 0f) Phase2FogOpacity -= 0.02f;
            bool bossActive = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<Punisher>())
                {
                    bossActive = true;
                    break;
                }
            }
            if (bossActive)
            {
                Main.cloudAlpha = 0f;
            }
            if (SpaceDistortionOpacity > 0f) SpaceDistortionOpacity -= 0.05f;
            if (Phase2SlashOpacity > 0f) Phase2SlashOpacity -= 0.05f;
            if (WhiteFlashOpacity > 0f)
            {
                WhiteFlashOpacity -= 0.008f;
            }
            if (!Main.gamePaused && WorldBlackoutOpacity > 0f)
            {
                BlackoutTimer++;
                Vector2 screenDelta = Main.screenPosition - LastScreenPosition;
                if (screenDelta.Length() > 1000f) screenDelta = Vector2.Zero;

                foreach (var shard in WorldShards)
                {
                    shard.DrawPosition += shard.Velocity;
                    shard.Rotation += shard.RotVelocity;
                    shard.DrawPosition -= screenDelta * shard.Parallax;

                    if (shard.DrawPosition.X < -1000) shard.DrawPosition.X = Main.screenWidth + 1000;
                    else if (shard.DrawPosition.X > Main.screenWidth + 1000) shard.DrawPosition.X = -1000;
                    if (shard.DrawPosition.Y < -1000) shard.DrawPosition.Y = Main.screenHeight + 1000;
                    else if (shard.DrawPosition.Y > Main.screenHeight + 1000) shard.DrawPosition.Y = -1000;
                }
            }
            else
            {
                BlackoutTimer = 0;
            }

            LastScreenPosition = Main.screenPosition;

            if (!NPC.AnyNPCs(ModContent.NPCType<Punisher>()))
            {
                BossAbsenceTimer++;
                if (BossAbsenceTimer > 60)
                {
                    if (WorldBlackoutOpacity > 0f) WorldBlackoutOpacity -= 0.05f;
                }
            }
            else
            {
                BossAbsenceTimer = 0;
            }

            VignetteIntensity = MathHelper.Clamp(VignetteIntensity, 0f, 1f);
            PurpleFogOpacity = MathHelper.Clamp(PurpleFogOpacity, 0f, 1f);
            UniverseTearOpacity = MathHelper.Clamp(UniverseTearOpacity, 0f, 1f);
            PainOpacity = MathHelper.Clamp(PainOpacity, 0f, 1f);
            WhiteFlashOpacity = MathHelper.Clamp(WhiteFlashOpacity, 0f, 1f);
            Phase2FogOpacity = MathHelper.Clamp(Phase2FogOpacity, 0f, 1f);
            SpaceDistortionOpacity = MathHelper.Clamp(SpaceDistortionOpacity, 0f, 1f);
            Phase2SlashOpacity = MathHelper.Clamp(Phase2SlashOpacity, 0f, 1f);
            KaleidoscopeOpacity = MathHelper.Clamp(KaleidoscopeOpacity, 0f, 1f);
            HexBarrierOpacity = MathHelper.Clamp(HexBarrierOpacity, 0f, 1f);
            
            UpdateFilter("jarkos:PunisherVignette", VignetteIntensity);
            UpdateFilter("jarkos:PunisherPurpleFog", PurpleFogOpacity);
            UpdateFilter("jarkos:PunisherMultiSlash", UniverseTearOpacity);
            UpdateFilter("jarkos:PunisherWhiteFlash", WhiteFlashOpacity);
            UpdateFilter("jarkos:PunisherPain", PainOpacity);
            UpdateFilter("jarkos:PunisherPhase2Fog", Phase2FogOpacity);
            UpdateFilter("jarkos:PunisherSpaceDistortion", SpaceDistortionOpacity);
            UpdateFilter("jarkos:PunisherPhase2Slash", Phase2SlashOpacity);
            UpdateFilter("jarkos:PunisherKaleidoscope", KaleidoscopeOpacity);
        }
        private void UpdateFilter(string filterName, float opacity)
        {
            if (opacity > 0f)
            {
                if (!Filters.Scene[filterName].IsActive())
                {
                    Filters.Scene.Activate(filterName);
                    Filters.Scene[filterName].Opacity = 1f;
                }
                Filters.Scene[filterName].GetShader().UseOpacity(opacity);
            }
            else
            {
                if (Filters.Scene[filterName].IsActive())
                {
                    Filters.Scene.Deactivate(filterName);
                }
            }
            if (Filters.Scene["jarkos:PunisherMultiSlash"].IsActive())
            {
                Effect effect = Filters.Scene["jarkos:PunisherMultiSlash"].GetShader().Shader;
                if (effect != null)
                {
                    if (effect.Parameters["uTearNormal"] != null)
                        effect.Parameters["uTearNormal"].SetValue(TearNormal);
                    if (effect.Parameters["uTearPoint"] != null)
                        effect.Parameters["uTearPoint"].SetValue(TearPoint);
                }
            }
            if (Filters.Scene["jarkos:PunisherSpaceDistortion"].IsActive())
            {
                Effect effect = Filters.Scene["jarkos:PunisherSpaceDistortion"].GetShader().Shader;
                if (effect != null)
                {
                    if (effect.Parameters["uTearNormal"] != null)
                        effect.Parameters["uTearNormal"].SetValue(DistortionNormal);
                    if (effect.Parameters["uTearPoint"] != null)
                        effect.Parameters["uTearPoint"].SetValue(DistortionPoint);
                }
            }
            if (Filters.Scene["jarkos:PunisherPhase2Slash"].IsActive())
            {
                Effect effect = Filters.Scene["jarkos:PunisherPhase2Slash"].GetShader().Shader;
                if (effect != null)
                {
                    if (effect.Parameters["uTearNormal"] != null)
                        effect.Parameters["uTearNormal"].SetValue(TearNormal);
                    if (effect.Parameters["uTearPoint"] != null)
                        effect.Parameters["uTearPoint"].SetValue(TearPoint);
                }
            }
            if (Filters.Scene["jarkos:PunisherKaleidoscope"].IsActive())
            {
                Filters.Scene["jarkos:PunisherKaleidoscope"].GetShader()
                    .UseTargetPosition(BossWorldPos)
                    .UseImageOffset(new Vector2(Main.screenWidth, Main.screenHeight));
            }
        }
    }
}
