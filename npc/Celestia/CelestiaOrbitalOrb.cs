using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using jarkos.particles;

namespace jarkos.npc.Celestia
{
    public class CelestiaOrbitalOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private static Texture2D blankTexture;
        private static readonly Color GlowCyan = new Color(0, 220, 255);
        private static readonly Color GlowMagenta = new Color(255, 60, 220);

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;
        }

        private float _currentAngle;
        private float _displayScale = 0f;

        public override void AI()
        {
            float radius = Projectile.ai[0];
            int arenaIndex = (int)Projectile.ai[2];

            if (arenaIndex < 0 || arenaIndex >= Main.maxProjectiles || !Main.projectile[arenaIndex].active || Main.projectile[arenaIndex].type != ModContent.ProjectileType<CelestiaOrbitalArena>())
            {
                Projectile.Kill();
                return;
            }

            Projectile arena = Main.projectile[arenaIndex];
            int arenaTimeLeft = arena.timeLeft;

            if (arenaTimeLeft > 60)
            {
                _displayScale = MathHelper.Lerp(_displayScale, 1.0f, 0.08f);
            }
            else
            {
                _displayScale = MathHelper.Lerp(_displayScale, 0f, 0.12f);
                if (_displayScale < 0.05f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Projectile.scale = _displayScale;

            if (Projectile.localAI[0] == 0)
            {
                _currentAngle = Projectile.ai[1];
                Projectile.localAI[0] = 1;
            }

            float radiusRatio = MathHelper.Clamp(radius / 1400f, 0.15f, 1.0f);
            float currentSpeed = 0.010f + radiusRatio * 0.032f;
            _currentAngle += currentSpeed;

            Projectile.Center = arena.Center + new Vector2((float)Math.Cos(_currentAngle), (float)Math.Sin(_currentAngle)) * radius;

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3) && _displayScale > 0.5f)
            {
                Color pColor = Main.rand.NextBool() ? GlowCyan : GlowMagenta;
                new StarParticle(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), Main.rand.NextVector2Circular(1.5f, 1.5f), pColor * 0.8f, 15, MathHelper.Clamp(Main.rand.NextFloat(0.1f, 0.35f) * _displayScale, 0.05f, 0.45f), 1f, 0f).Spawn();
            }

            Lighting.AddLight(Projectile.Center, 0.6f * _displayScale, 0.2f * _displayScale, 0.8f * _displayScale);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = 35f * _displayScale;
            Vector2 closest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.DistanceSquared(Projectile.Center, closest) < radius * radius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_displayScale <= 0.01f) return false;

            float charge = 0.9f;

            Effect shader = ModContent.Request<Effect>("jarkos/Effects/CelestiaBallLightning", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.8f);
            shader.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 6.43f);
            shader.Parameters["uOpacity"]?.SetValue(_displayScale);
            shader.Parameters["uCharge"]?.SetValue(charge);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader, Main.GameViewMatrix.ZoomMatrix);

            float size = (140f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f + Projectile.whoAmI) * 8f) * _displayScale;
            Main.spriteBatch.Draw(blankTexture, Projectile.Center - Main.screenPosition, null, Color.White, 0f, new Vector2(0.5f), new Vector2(size), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            DrawOuterArcs(Main.spriteBatch);

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, GlowMagenta * (0.65f * _displayScale), 0f, bloom.Size() * 0.5f, 0.5f * _displayScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        private void DrawOuterArcs(SpriteBatch sb)
        {
            const int arcLife = 12;

            int cycle = (int)(Main.GameUpdateCount / arcLife);
            float frac = Main.GameUpdateCount % arcLife / (float)arcLife;
            float fade = (float)Math.Sin(frac * Math.PI);

            for (int i = 0; i < 3; i++)
            {
                var rand = new Random(Projectile.whoAmI * 223 + i * 617 + cycle * 37);
                if (rand.NextDouble() < 0.3) continue;

                float ang = (float)(rand.NextDouble() * MathHelper.TwoPi);
                float len = (45f + (float)rand.NextDouble() * 45f) * _displayScale;

                Vector2 start = Projectile.Center + ang.ToRotationVector2() * 15f * _displayScale;
                Vector2 end = start + ang.ToRotationVector2() * len;

                DrawLightningBolt(sb, start, end, rand, fade * _displayScale);
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
                float amp = (1f - Math.Abs(t - 0.5f) * 2f) * dir.Length() * 0.12f;

                Vector2 point = start + dir * t + perp * (float)(rand.NextDouble() * 2.0 - 1.0) * amp;
                
                if (i == segs) point = end;

                DrawLine(sb, prev, point, GlowMagenta * (0.5f * intensity), 6f);
                DrawLine(sb, prev, point, GlowCyan * (0.6f * intensity), 2.8f);
                DrawLine(sb, prev, point, Color.White * (0.9f * intensity), 1.4f);
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
