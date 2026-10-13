using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;

namespace jarkos.projectiles.magic
{
    public class SpectralConvergenceProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Phantom_proj02";

        private Vector2 targetPoint;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0f)
            {
                targetPoint = Projectile.Center + Projectile.velocity;
                Projectile.ai[0] = 1f;
            }

            Projectile.ai[1]++;

            if (Projectile.ai[1] <= 15f)
            {
                Vector2 dirToTarget = Vector2.Normalize(targetPoint - Projectile.Center);
                Projectile.rotation = dirToTarget.ToRotation() + MathHelper.PiOver2;

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                {
                    new SpectralPlasmaParticle(Projectile.Center, dirToTarget * 2f, Color.Cyan, Color.DeepSkyBlue, 12, 0.4f, 0.8f).Spawn();
                }
            }
            else
            {
                Vector2 dirToTarget = targetPoint - Projectile.Center;
                float distance = dirToTarget.Length();

                if (distance < 20f || Projectile.ai[1] > 35f)
                {
                    Projectile.Kill();
                    return;
                }

                dirToTarget.Normalize();
                Projectile.velocity = dirToTarget * 26f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Main.netMode != NetmodeID.Server)
                {
                    new SpectralPlasmaParticle(Projectile.Center, -Projectile.velocity * 0.1f, Color.Cyan, Color.Turquoise, 14, 0.5f, 0.9f).Spawn();
                    new Spark(Projectile.Center, -Projectile.velocity * 0.15f, Color.White, 12, 0.35f, 0.85f).Spawn();
                }
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 1.0f);
        }

        public override void OnKill(int timeLeft)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<Screenshake_system>();
            if (modPlayer != null)
            {
                modPlayer.weak_shake = 3.5f;
            }

            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/phantom_javelin_impact") with { Pitch = 0.5f, Volume = 0.9f }, Projectile.position);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 18; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(8f, 8f);
                    new SpectralPlasmaParticle(Projectile.Center, burstVel, Color.Cyan, Color.DeepSkyBlue, 25, Main.rand.NextFloat(0.4f, 0.8f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }

                for (int i = 0; i < 8; i++)
                {
                    Vector2 starVel = Main.rand.NextVector2Circular(5f, 5f);
                    new StarParticle(Projectile.Center, starVel, Color.White, 20, 0.35f, 1f, Main.rand.NextFloat(-0.3f, 0.3f)).Spawn();
                }

                new SpectralRingParticle(Projectile.Center, Color.Cyan, 20, 0.1f, 0.85f, 0.95f).Spawn();

                int fireCount = Main.rand.Next(2, 4);
                for (int i = 0; i < fireCount; i++)
                {
                    Vector2 fVel = Main.rand.NextVector2Circular(8f, 8f);
                    new Fire_particle(Projectile.Center, fVel, Color.Cyan, 30, Main.rand.NextFloat(0.8f, 1.3f), 1f, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
                }

                int elecCount = Main.rand.Next(2, 4);
                for (int i = 0; i < elecCount; i++)
                {
                    Vector2 eVel = Main.rand.NextVector2Circular(10f, 10f);
                    new Electric_particle(Projectile.Center, eVel, new Color(50, 200, 255), Main.rand.Next(20, 32), Main.rand.NextFloat(0.45f, 0.75f), 1f, 0.92f).Spawn();
                }
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                Color color = Color.Cyan * progress * 0.7f;
                float scale = Projectile.scale * (0.85f + 0.35f * progress);
                float rot = Projectile.oldRot[k] != 0f ? Projectile.oldRot[k] : Projectile.rotation;

                spriteBatch.Draw(texture, drawPos, null, color, rot, drawOrigin, scale, SpriteEffects.None, 0f);
            }

            Vector2 mainDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            spriteBatch.Draw(texture, mainDrawPos, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            return false;
        }
    }
}
