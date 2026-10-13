using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaPhase3Aura : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private const int FadeInTicks = 120;
        private const int FadeOutTicks = 60;
        private const int PulsePeriod = 150;
        private const float AuraIntensity = 0.55f;

        public ref float BossIndex => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.localAI[0];

        private float dyingProgress;
        private bool dying;

        private static Texture2D blankTexture;
        private static readonly Color Violet = new Color(150, 60, 255);
        private static readonly Color DeepMagenta = new Color(220, 40, 160);
        private static readonly Color GlowBlue = new Color(80, 170, 255);

        private float Fade => MathHelper.Clamp(Timer / FadeInTicks, 0f, 1f) * (1f - dyingProgress);
        private float PulseProgress => Timer % PulsePeriod / PulsePeriod;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            NPC boss = Main.npc.IndexInRange((int)BossIndex) ? Main.npc[(int)BossIndex] : null;

            if (boss == null || !boss.active || boss.type != ModContent.NPCType<Celestia>())
                dying = true;

            if (dying)
            {
                dyingProgress += 1f / FadeOutTicks;
                Projectile.timeLeft = 60;

                if (dyingProgress >= 1f)
                {
                    Projectile.Kill();
                    return;
                }

                float dim = Fade;
                Lighting.AddLight(Projectile.Center, 0.7f * dim, 0.3f * dim, 1.2f * dim);
                return;
            }

            Timer++;
            Projectile.timeLeft = 60;
            Projectile.Center = boss.Center;

            if ((int)Timer % PulsePeriod == 0 && Timer > FadeInTicks)
            {
                SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.45f, Pitch = -0.4f }, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 2.5f, 6f, 10, 2000f, "CelestiaAura"));

                if (Main.netMode != NetmodeID.Server)
                    for (int i = 0; i < 12; i++)
                        new Spark(Projectile.Center, (MathHelper.TwoPi / 12f * i).ToRotationVector2() * Main.rand.NextFloat(6f, 9f), i % 2 == 0 ? Violet : DeepMagenta, 24, 0.55f, 1f, 0f).Spawn();
            }

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(4))
            {
                Vector2 from = Projectile.Center + Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(240f, 380f);
                Dust d = Dust.NewDustPerfect(from, Main.rand.NextBool(3) ? DustID.Electric : DustID.PurpleTorch, (Projectile.Center - from).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(5f, 9f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }

            float glow = Fade * (0.8f + 0.3f * (float)Math.Sin(Timer * 0.08f));
            Lighting.AddLight(Projectile.Center, 0.7f * glow, 0.3f * glow, 1.2f * glow);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            float fade = Fade;
            if (fade <= 0.01f)
                return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Effect aura = ModContent.Request<Effect>("jarkos/Effects/CelestiaPhase3Aura", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            aura.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            aura.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 5.17f);
            aura.Parameters["uFade"]?.SetValue(fade);
            aura.Parameters["uPulse"]?.SetValue(PulseProgress);
            aura.Parameters["uMaster"]?.SetValue(AuraIntensity);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, aura, Main.GameViewMatrix.ZoomMatrix);

            float breath = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.04f;
            float size = 760f * fade * breath;
            Main.spriteBatch.Draw(blankTexture, drawPos, null, Color.White, 0f, new Vector2(0.5f), new Vector2(size), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            float t = Main.GlobalTimeWrappedHourly;
            float vis = fade * AuraIntensity;

            for (int i = 0; i < 3; i++)
            {
                float ang = t * (0.9f + i * 0.35f) * (i % 2 == 0 ? 1f : -1f) + i * MathHelper.TwoPi / 3f;
                float rx = (160f + i * 40f) * fade;
                float ry = rx * (0.45f + i * 0.12f);
                Vector2 shardPos = drawPos + new Vector2((float)Math.Cos(ang) * rx, (float)Math.Sin(ang) * ry);

                float shardPulse = 0.8f + 0.2f * (float)Math.Sin(t * 7f + i * 2.1f);
                Color shardCol = i == 0 ? Violet : (i == 1 ? DeepMagenta : GlowBlue);

                Main.spriteBatch.Draw(bloom, shardPos, null, shardCol * (0.55f * vis), 0f, bloom.Size() * 0.5f, 0.38f * shardPulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, shardPos, null, Color.White * (0.45f * vis), 0f, bloom.Size() * 0.5f, 0.15f * shardPulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, shardPos, null, shardCol * (0.3f * vis), ang + MathHelper.PiOver2, bloom.Size() * 0.5f, new Vector2(1.0f, 0.18f) * shardPulse, SpriteEffects.None, 0f);
            }

            DrawAuraArcs(Main.spriteBatch, vis);

            Main.spriteBatch.Draw(bloom, drawPos, null, Violet * (0.3f * vis), 0f, bloom.Size() * 0.5f, 1.5f * fade, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
        private void DrawAuraArcs(SpriteBatch sb, float vis)
        {
            const int arcLife = 18;

            int cycle = (int)(Main.GameUpdateCount / arcLife);
            float frac = Main.GameUpdateCount % arcLife / (float)arcLife;
            float arcFade = (float)Math.Sin(frac * Math.PI) * vis;

            for (int i = 0; i < 3; i++)
            {
                var rand = new Random(Projectile.whoAmI * 211 + i * 883 + cycle * 47);
                if (rand.NextDouble() < 0.5) continue;

                float ang = (float)(rand.NextDouble() * MathHelper.TwoPi);
                float len = 80f + (float)rand.NextDouble() * 90f;
                Vector2 start = Projectile.Center + ang.ToRotationVector2() * 120f * Fade;
                Vector2 end = start + ang.ToRotationVector2() * len * Fade;

                DrawLightningBolt(sb, start, end, rand, 0.55f * arcFade);
            }
        }

        private static void DrawLightningBolt(SpriteBatch sb, Vector2 start, Vector2 end, Random rand, float intensity)
        {
            const int segs = 9;

            Vector2 dir = end - start;
            Vector2 perp = dir.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            Vector2 prev = start;

            for (int i = 1; i <= segs; i++)
            {
                float t = i / (float)segs;
                float amp = (1f - Math.Abs(t - 0.5f) * 2f) * dir.Length() * 0.13f;

                Vector2 point = start + dir * t + perp * (float)(rand.NextDouble() * 2.0 - 1.0) * amp;
                
                if (i == segs) point = end;

                DrawLine(sb, prev, point, Violet * (0.35f * intensity), 6f);
                DrawLine(sb, prev, point, DeepMagenta * (0.45f * intensity), 2.5f);
                DrawLine(sb, prev, point, Color.White * (0.8f * intensity), 1.2f);
                prev = point;
            }
        }

        private static void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, Color color, float width)
        {
            Vector2 d = b - a;
            sb.Draw(blankTexture, a - Main.screenPosition, null, color, d.ToRotation(), new Vector2(0f, 0.5f), new Vector2(d.Length(), width), SpriteEffects.None, 0f);
        }
    }
}
