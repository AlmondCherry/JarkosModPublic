using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaPhase3Transition : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private const int Lifetime = 960;
        private const int GripEnd = 90;
        private const int SuctionEnd = 720;
        private const int WhiteEnd = 810;
        private const int ShockEnd = 890;
        private const int CameraReleaseTicks = 40;
        private const int ProjectileClearWindow = 10;

        public ref float BossIndex => ref Projectile.ai[0];
        private float Timer => Lifetime - Projectile.timeLeft;
        private Vector2 frozenFocus;
        private bool focusFrozen;

        private static Texture2D blankTexture;
        private static readonly Color Violet = new Color(150, 60, 255);
        private static readonly Color GlowBlue = new Color(80, 170, 255);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            NPC boss = Main.npc.IndexInRange((int)BossIndex) ? Main.npc[(int)BossIndex] : null;
            if (boss == null || !boss.active)
            {
                EndTransition();
                Projectile.Kill();
                return;
            }

            float t = Timer;

            boss.velocity *= 0.9f;
            boss.dontTakeDamage = true;
            Projectile.Center = boss.Center;

            if (t >= SuctionEnd && !focusFrozen)
            {
                focusFrozen = true;
                frozenFocus = boss.Center;
            }
            Vector2 focus = focusFrozen ? frozenFocus : boss.Center;

            CelestiaTransitionSystem.TransitionActive = true;
            CelestiaTransitionSystem.FocusPoint = focus;

            if (t < GripEnd)
            {
                CelestiaTransitionSystem.CameraLerp = MathHelper.SmoothStep(0f, 1f, t / GripEnd);
            }
            else if (t >= ShockEnd)
            {
                float rel = MathHelper.Clamp((t - ShockEnd) / CameraReleaseTicks, 0f, 1f);
                CelestiaTransitionSystem.CameraLerp = MathHelper.SmoothStep(1f, 0f, rel);

                if (rel >= 1f)
                    CelestiaTransitionSystem.CameraLerp = 0f;
            }
            else
            {
                CelestiaTransitionSystem.CameraLerp = 1f;
            }

            CelestiaTransitionSystem.SuctionPhase = t >= GripEnd && t < WhiteEnd - 30;

            CelestiaTransitionSystem.SuctionIntensity = ComputeSuction(t);
            CelestiaTransitionSystem.Whiteout = ComputeWhiteout(t);

            if (t >= WhiteEnd && t <= WhiteEnd + ProjectileClearWindow)
                ClearHostileProjectiles();

            RunTimeline(boss, t);
            ManageFilters(t, focus);

            if (Projectile.timeLeft <= 1)
                EndTransition();
        }

        private void ClearHostileProjectiles()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.hostile && p.whoAmI != Projectile.whoAmI)
                    p.Kill();
            }
        }

        private float ComputeSuction(float t)
        {
            if (t < GripEnd) return MathHelper.Lerp(0f, 0.15f, t / GripEnd);
            if (t < SuctionEnd) return MathHelper.Lerp(0.15f, 1f, (t - GripEnd) / (SuctionEnd - GripEnd));
            if (t < WhiteEnd) return 1f;
            return 0f;
        }

        private float ComputeWhiteout(float t)
        {
            if (t < SuctionEnd) return 0f;
            if (t < WhiteEnd) return MathHelper.SmoothStep(0f, 1f, (t - SuctionEnd) / (WhiteEnd - SuctionEnd));
            if (t < WhiteEnd + 40) return 1f - (t - WhiteEnd) / 40f;
            return 0f;
        }

        private void ManageFilters(float t, Vector2 focus)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (t < WhiteEnd)
            {
                if (!Filters.Scene[CelestiaTransitionSystem.SuctionFilter].IsActive())
                    Filters.Scene.Activate(CelestiaTransitionSystem.SuctionFilter, focus);

                Filters.Scene[CelestiaTransitionSystem.SuctionFilter].GetShader()
                    .UseTargetPosition(focus)
                    .UseIntensity(CelestiaTransitionSystem.SuctionIntensity)
                    .UseProgress(t / WhiteEnd);
            }
            else
            {
                if (Filters.Scene[CelestiaTransitionSystem.SuctionFilter].IsActive())
                    Filters.Scene[CelestiaTransitionSystem.SuctionFilter].Deactivate();

                float shock = MathHelper.Clamp((t - WhiteEnd) / (float)(ShockEnd - WhiteEnd), 0f, 1f);
                CelestiaTransitionSystem.ShockProgress = t < Lifetime - 5 ? shock : -1f;

                if (!Filters.Scene[CelestiaTransitionSystem.ShockFilter].IsActive())
                    Filters.Scene.Activate(CelestiaTransitionSystem.ShockFilter, focus);

                Filters.Scene[CelestiaTransitionSystem.ShockFilter].GetShader()
                    .UseTargetPosition(focus)
                    .UseProgress(shock)
                    .UseIntensity(1f - shock);

                if (t >= Lifetime - 5 && Filters.Scene[CelestiaTransitionSystem.ShockFilter].IsActive())
                    Filters.Scene[CelestiaTransitionSystem.ShockFilter].Deactivate();
            }
        }

        private void RunTimeline(NPC boss, float t)
        {
            if (t < GripEnd)
            {
                if (t == 1f)
                {
                    SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 1.4f, Pitch = -0.6f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.8f }, Projectile.Center);
                }
                if ((int)t % 12 == 0)
                    Shake(3f, 10, 3000f);
            }
            else if (t < SuctionEnd)
            {
                if (t == GripEnd)
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.4f, Pitch = -0.7f }, Projectile.Center);

                if ((int)t % 8 == 0)
                    Shake(4f + CelestiaTransitionSystem.SuctionIntensity * 4f, 10, 3500f);

                float prog = (t - GripEnd) / (SuctionEnd - GripEnd);
                int interval = (int)MathHelper.Lerp(10f, 4f, prog);

                if (Main.netMode != NetmodeID.MultiplayerClient && (int)t % interval == 0)
                {
                    int burst = prog > 0.5f ? 2 : 1;
                    for (int s = 0; s < burst; s++)
                        SpawnStar(boss);
                }

                if (Main.netMode != NetmodeID.Server)
                    for (int i = 0; i < 2 + (int)(prog * 3f); i++)
                    {
                        Vector2 from = boss.Center + Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(400f, 1100f);
                        Dust d = Dust.NewDustPerfect(from, Main.rand.NextBool() ? DustID.Electric : DustID.PurpleTorch, (boss.Center - from).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(14f, 26f));
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1.2f, 2.2f);
                    }
            }
            else if (t < WhiteEnd)
            {
                if (t == SuctionEnd)
                    SoundEngine.PlaySound(SoundID.Item117 with { Volume = 1.3f, Pitch = -0.9f }, Projectile.Center);
                if ((int)t % 4 == 0)
                    Shake(7f, 8, 3500f);
            }
            else if (t < ShockEnd)
            {
                if (t == WhiteEnd)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.6f, Pitch = -1f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Thunder with { Volume = 1.5f, Pitch = -0.5f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 1.5f, Pitch = -0.9f }, Projectile.Center);

                    Shake(45f, 70, 6000f);
                    Shake(30f, 50, 6000f);
                    Shake(18f, 90, 6000f);

                    CelestiaTransitionSystem.KnockbackFrom = boss.Center;
                    CelestiaTransitionSystem.KnockbackTick = Main.GameUpdateCount;

                    if (Main.netMode != NetmodeID.Server)
                        for (int i = 0; i < 80; i++)
                            new Spark(boss.Center, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(10f, 34f), i % 3 == 0 ? Color.White : (i % 3 == 1 ? GlowBlue : Violet), 45, 1f, 1f, 0f).Spawn();
                }
                if ((int)t % 4 == 0)
                    Shake(10f * (1f - (t - WhiteEnd) / (ShockEnd - WhiteEnd)), 10, 5000f);
            }
            else
            {
                if (t == ShockEnd)
                    SoundEngine.PlaySound(SoundID.Zombie105 with { Volume = 1f, Pitch = 0.2f }, Projectile.Center);
            }

            Lighting.AddLight(Projectile.Center, 1.5f, 1f, 3f);
        }

        private void SpawnStar(NPC boss)
        {
            float halfW = Main.screenWidth * 0.8f + 350f;
            float halfH = Main.screenHeight * 0.8f + 350f;

            Vector2 edge;
            if (Main.rand.NextBool())
                edge = new Vector2(Main.rand.NextBool() ? -halfW : halfW, Main.rand.NextFloat(-halfH, halfH));
            else
                edge = new Vector2(Main.rand.NextFloat(-halfW, halfW), Main.rand.NextBool() ? -halfH : halfH);

            Vector2 spawnPos = boss.Center + edge;
            Vector2 vel = (boss.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(9f, 13f);
            vel = vel.RotatedByRandom(0.35);

            Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPos, vel, ModContent.ProjectileType<CelestiaSuckedStar>(), 90, 4f, Main.myPlayer, boss.Center.X, boss.Center.Y);
        }

        private void Shake(float strength, int frames, float radius)
        {
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), strength, 8f, frames, radius, "CelestiaTransition"));
        }

        private void EndTransition()
        {
            CelestiaTransitionSystem.TransitionActive = false;
            NPC boss = Main.npc.IndexInRange((int)BossIndex) ? Main.npc[(int)BossIndex] : null;
            if (boss != null && boss.active)
                boss.dontTakeDamage = false;
        }

        public override void OnKill(int timeLeft) => EndTransition();

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            float suck = CelestiaTransitionSystem.SuctionIntensity;
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.1f;
            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, Violet * (0.6f * suck), 0f, bloom.Size() * 0.5f, (1.5f + suck * 2f) * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            float white = CelestiaTransitionSystem.Whiteout;
            if (white > 0f)
                Main.spriteBatch.Draw(blankTexture, new Rectangle(-50, -50, (int)(Main.screenWidth / Main.GameViewMatrix.Zoom.X) + 100, (int)(Main.screenHeight / Main.GameViewMatrix.Zoom.Y) + 100), Color.White * white);

            return false;
        }
    }
}
