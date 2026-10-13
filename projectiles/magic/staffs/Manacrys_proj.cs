using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.particles;
using Terraria.Audio;

namespace jarkos.projectiles.magic.staffs
{
    public class Manacrys_proj : ModProjectile
    {
        private float BaseScale => Projectile.ai[0] > 0f ? Projectile.ai[0] : 1f;
        private ref float DyingTimer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
            Projectile.light = 0.5f;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 15;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            if (DyingTimer > 0f)
            {
                DyingTimer++;
                Projectile.velocity = Vector2.Zero;
                if (DyingTimer >= 31f)
                {
                    int starCount = Main.rand.Next(1, 3);
                    float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < starCount; i++)
                    {
                        float angle = baseAngle + (i / (float)starCount) * MathHelper.TwoPi + Main.rand.NextFloat(-0.35f, 0.35f);
                        Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(6.5f, 10.5f);
                        float scale = Main.rand.NextFloat(0.18f, 0.38f);
                        int lifetime = Main.rand.Next(25, 45);
                        float spin = Main.rand.NextFloat(-0.25f, 0.25f);
                        new StarParticle(Projectile.Center, starVel, new Color(255, 40, 40), lifetime, scale, 1f, spin).Spawn();
                    }

                    int sparkCount = Main.rand.Next(4, 7);
                    for (int i = 0; i < sparkCount; i++)
                    {
                        float angle = (i / (float)sparkCount) * MathHelper.TwoPi + Main.rand.NextFloat(-0.2f, 0.2f);
                        Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5.0f, 9.0f);
                        float scale = Main.rand.NextFloat(0.6f, 1.1f);
                        int lifetime = Main.rand.Next(20, 35);
                        new Spark(Projectile.Center, sparkVel, Color.White, lifetime, scale, 0.95f).Spawn();
                    }

                    Projectile.Kill();
                }
                return;
            }

            Projectile.velocity *= 0.94f;

            float speed = Projectile.velocity.Length();
            float spinSpeed = 0.02f + 0.015f * speed;
            Projectile.rotation += spinSpeed;

            if (speed < 1.4f || Projectile.timeLeft <= 31)
            {
                TriggerDyingAnimation();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TriggerDyingAnimation();
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            TriggerDyingAnimation();
        }

        private void TriggerDyingAnimation()
        {
            if (DyingTimer > 0f) return;
            DyingTimer = 1f;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ManaCrystalBurst") with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, Projectile.Center);
            new Circular_glow(Projectile.Center, Vector2.Zero, new Color(255, 140, 20), 32, BaseScale * 0.55f, 0.9f).Spawn();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float baseScale = BaseScale;
            float spawnOpacity = (255 - Projectile.alpha) / 255f;

            Texture2D glow = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glow.Size() * 0.5f;

            if (DyingTimer > 0f)
            {
                float progress = MathHelper.Clamp((DyingTimer - 1f) / 30f, 0f, 1f);
                float easedProgress = 1f - MathF.Pow(1f - progress, 3f);

                float scaleX = MathHelper.Lerp(baseScale, baseScale * 0.04f, easedProgress);
                float scaleY = MathHelper.Lerp(baseScale, baseScale * 3.2f, easedProgress);
                Vector2 drawScale = new Vector2(scaleX, scaleY);

                float opacity = (1f - easedProgress) * spawnOpacity;
                Color starColor = Color.Lerp(Color.White, new Color(255, 40, 30), easedProgress) * opacity;

                Color orangeGlowColor = Color.Lerp(new Color(255, 140, 20), new Color(255, 60, 10), easedProgress) * opacity * 0.85f;
                Vector2 glowScale = new Vector2(scaleX * 1.6f, scaleY * 1.1f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(glow, drawPos, null, orangeGlowColor, Projectile.rotation, glowOrigin, glowScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, drawPos, null, starColor, Projectile.rotation, drawOrigin, drawScale, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                return false;
            }

            Color mainOrangeGlow = (new Color(255, 140, 20) * 0.65f) * spawnOpacity;
            Vector2 mainGlowScale = new Vector2(baseScale * 0.55f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glow, drawPos, null, mainOrangeGlow, Projectile.rotation * 0.5f, glowOrigin, mainGlowScale, SpriteEffects.None, 0);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;
                Vector2 oldDrawPos = (Projectile.oldPos[k] - Main.screenPosition) + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
                float trailProgress = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                Color trailColor = (new Color(120, 200, 255, 0) * (0.35f * trailProgress)) * spawnOpacity;
                Main.EntitySpriteDraw(texture, oldDrawPos, null, trailColor, Projectile.rotation, drawOrigin, baseScale * trailProgress, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, Color.White * spawnOpacity, Projectile.rotation, drawOrigin, baseScale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
