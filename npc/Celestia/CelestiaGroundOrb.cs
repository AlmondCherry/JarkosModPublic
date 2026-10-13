using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using jarkos.particles;

namespace jarkos.npc.Celestia
{
    public class CelestiaGroundOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private static Texture2D blankTexture;
        private static readonly Color GlowCyan = new Color(0, 220, 255);
        private static readonly Color GlowPurple = new Color(210, 80, 255);
        public ref float TargetY => ref Projectile.ai[0];
        private bool _isSlowing = false;

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Projectile.velocity = new Vector2(0f, -38f - Main.rand.NextFloat(0f, 6f));
                Projectile.scale = 0.3f;
            }

            if (!_isSlowing && TargetY != 0 && Projectile.Center.Y <= TargetY)
            {
                _isSlowing = true;
                Projectile.localAI[2] = Main.rand.Next(120, 301);
            }

            if (_isSlowing)
            {
                Projectile.velocity *= 0.88f;

                Projectile.localAI[1]++;
                float maxLife = Projectile.localAI[2] > 0 ? Projectile.localAI[2] : 200f;
                float progress = MathHelper.Clamp(Projectile.localAI[1] / maxLife, 0f, 1f);

                Projectile.scale = MathHelper.Lerp(1.0f, 3.2f, progress);

                Vector2 oldCenter = Projectile.Center;
                int currentSize = (int)(52f * Projectile.scale);
                Projectile.width = currentSize;
                Projectile.height = currentSize;
                Projectile.Center = oldCenter;

                if (Main.netMode != NetmodeID.Server && Main.rand.NextFloat() < (0.35f + progress * 0.55f))
                {
                    Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(30f * Projectile.scale, 30f * Projectile.scale);
                    Color pColor = Main.rand.NextBool() ? GlowCyan : GlowPurple;
                    new StarParticle(particlePos, Main.rand.NextVector2Circular(3f, 3f), pColor, 18, MathHelper.Clamp(Main.rand.NextFloat(0.15f, 0.4f) * Projectile.scale, 0.05f, 0.55f), 1f, 0f).Spawn();
                }

                if (Projectile.localAI[1] >= maxLife)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                if (Projectile.scale < 1.0f)
                    Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.0f, 0.12f);
            }

            Lighting.AddLight(Projectile.Center, 0.5f * Projectile.scale, 0.8f * Projectile.scale, 1.2f * Projectile.scale);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float arcReach = _isSlowing ? (40f + 40f * (Projectile.scale / 3.2f)) : 25f;
            float totalRadius = (24f * Projectile.scale) + arcReach;

            Vector2 closest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.DistanceSquared(Projectile.Center, closest) < totalRadius * totalRadius;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.4f, Volume = 0.8f }, Projectile.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CelestiaLightningExplosion>(), Projectile.damage, 1f, Main.myPlayer, ai0: Projectile.scale);
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 35; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(14f * Projectile.scale, 14f * Projectile.scale);
                    Color burstColor = Main.rand.NextBool() ? GlowCyan : GlowPurple;
                    if (i % 3 == 0) burstColor = Color.White;

                    new StarParticle(Projectile.Center, burstVel, burstColor, 30, MathHelper.Clamp(Main.rand.NextFloat(0.2f, 0.45f) * (Projectile.scale * 0.4f), 0.05f, 0.45f), 1f, Main.rand.NextFloat(-0.3f, 0.3f)).Spawn();
                }

                for (int i = 0; i < 15; i++)
                {
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(12f * Projectile.scale, 12f * Projectile.scale), GlowCyan, 22, 0.7f * Projectile.scale, 1f, 0f).Spawn();
                }

                for (int i = 0; i < 18; i++)
                {
                    Vector2 edgeVel = Main.rand.NextVector2CircularEdge(12f * Projectile.scale, 12f * Projectile.scale);
                    new Circleedge(Projectile.Center, edgeVel, GlowCyan, 32, 1.6f * Projectile.scale, 1f, 0f).Spawn();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (blankTexture == null)
            {
                blankTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                blankTexture.SetData(new[] { Color.White });
            }

            if (Projectile.scale <= 0.01f) return false;

            float charge = _isSlowing ? MathHelper.Clamp(Projectile.localAI[1] / (Projectile.localAI[2] > 0 ? Projectile.localAI[2] : 200f), 0.2f, 1f) : 0.8f;

            Effect shader = ModContent.Request<Effect>("jarkos/Effects/CelestiaBallLightning", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
            shader.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 9.17f);
            shader.Parameters["uOpacity"]?.SetValue(Projectile.scale);
            shader.Parameters["uCharge"]?.SetValue(charge);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, shader, Main.GameViewMatrix.ZoomMatrix);

            float size = (160f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 7f + Projectile.whoAmI) * 10f) * Projectile.scale;
            Main.spriteBatch.Draw(blankTexture, Projectile.Center - Main.screenPosition, null, Color.White, 0f, new Vector2(0.5f), new Vector2(size), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            DrawOuterArcs(Main.spriteBatch, charge);

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, GlowCyan * (0.6f * Projectile.scale), 0f, bloom.Size() * 0.5f, 0.55f * Projectile.scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        private void DrawOuterArcs(SpriteBatch sb, float charge)
        {
            const int arcLife = 14;
            int cycle = (int)(Main.GameUpdateCount / arcLife);

            float frac = Main.GameUpdateCount % arcLife / (float)arcLife;
            float fade = (float)Math.Sin(frac * Math.PI);

            int count = 4 + (int)(charge * 3f);
            for (int i = 0; i < count; i++)
            {
                var rand = new Random(Projectile.whoAmI * 173 + i * 821 + cycle * 43);
                if (rand.NextDouble() < 0.25) continue;

                float ang = (float)(rand.NextDouble() * MathHelper.TwoPi);
                float len = (55f + (float)rand.NextDouble() * 55f + charge * 30f) * Projectile.scale;
                Vector2 start = Projectile.Center + ang.ToRotationVector2() * 20f * Projectile.scale;
                Vector2 end = start + ang.ToRotationVector2() * len;

                DrawLightningBolt(sb, start, end, rand, fade * Projectile.scale);
            }
        }

        private static void DrawLightningBolt(SpriteBatch sb, Vector2 start, Vector2 end, Random rand, float intensity)
        {
            const int segs = 8;
            
            Vector2 dir = end - start;
            Vector2 perp = dir.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            Vector2 prev = start;

            for (int i = 1; i <= segs; i++)
            {
                float t = i / (float)segs;
                float amp = (1f - Math.Abs(t - 0.5f) * 2f) * dir.Length() * 0.14f;
                Vector2 point = start + dir * t + perp * (float)(rand.NextDouble() * 2.0 - 1.0) * amp;
                if (i == segs) point = end;

                DrawLine(sb, prev, point, GlowCyan * (0.5f * intensity), 7f);
                DrawLine(sb, prev, point, GlowPurple * (0.6f * intensity), 3f);
                DrawLine(sb, prev, point, Color.White * (0.9f * intensity), 1.5f);
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
