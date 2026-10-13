using jarkos;
using jarkos.buffs.debuff;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class M_blast_orb : ModProjectile
	{
        private static readonly Color MesonGreen = new Color(50, 255, 100);
        private static readonly Color MesonGreenBright = new Color(150, 255, 180);
        private const int TrailLength = 25;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float oscillation = (float)Math.Sin(Projectile.timeLeft * 0.4f) * 0.015f;
            Projectile.velocity = Projectile.velocity.RotatedBy(oscillation);
            Lighting.AddLight(Projectile.Center, 0.4f, 1f, 0.4f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
            if (Main.rand.NextBool(6))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.Electric, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 0, MesonGreen, 0.5f);
                Spark.noGravity = true;
            }
            if (Projectile.timeLeft % 30 == 0 && Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.2f, Pitch = 0.8f }, Projectile.Center);
                Projectile.soundDelay = 30;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawTrail(spriteBatch, pixel);
            DrawGlow(spriteBatch);
            DrawCore(spriteBatch, pixel);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2[] positions = Projectile.oldPos;
            for (int i = 1; i < TrailLength; i++)
            {
                if (positions[i] == Vector2.Zero || positions[i - 1] == Vector2.Zero)
                    continue;
                Vector2 currentPos = positions[i] + Projectile.Size / 2f;
                Vector2 previousPos = positions[i - 1] + Projectile.Size / 2f;
                float progress = 1f - (float)i / TrailLength;
                float width = 10f * progress;
                float alpha = progress * 0.8f;
                Color trailColor = Color.Lerp(MesonGreen, MesonGreenBright, progress * 0.5f) * alpha;
                Vector2 direction = previousPos - currentPos;
                float length = direction.Length();
                if (length > 0)
                {
                    float rotation = direction.ToRotation() + MathHelper.PiOver2;
                    spriteBatch.Draw(pixel, currentPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0.5f, 0f), new Vector2(width, length), SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, currentPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * alpha * 0.6f, rotation, new Vector2(0.5f, 0f), new Vector2(width * 0.4f, length), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawGlow(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.15f;
            spriteBatch.Draw(glowTex, drawPos, null, MesonGreen * 0.4f, 0f, glowTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, MesonGreenBright * 0.7f, 0f, glowTex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
        }
        private void DrawCore(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White, rotation, new Vector2(0.5f), new Vector2(6f, 14f), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), MesonGreen, rotation, new Vector2(0.5f), new Vector2(10f, 18f), SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.4f);
                Dust dust = Dust.NewDustDirect(target.Center - new Vector2(8), 16, 16, DustID.GreenTorch, dustVel.X, dustVel.Y, 100, default, 1.4f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.6f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.4f, Pitch = -0.2f }, Projectile.Center);
            int fragmentCount = 6;
            for (int i = 0; i < fragmentCount; i++)
            {
                float angle = MathHelper.TwoPi / fragmentCount * i + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = new Vector2(6f, 0f).RotatedBy(angle);
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, velocity, ModContent.ProjectileType<M_blast_proj>(), Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(3f, 7f), 0f).RotatedByRandom(MathHelper.TwoPi);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GreenTorch, dustVel.X, dustVel.Y, 50, default, 2f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12 * i;
                Vector2 dustVel = new Vector2(4f, 0f).RotatedBy(angle);
                Dust ring = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 0, MesonGreen, 1f);
                ring.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 1.2f, 2f, 1.2f);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return MesonGreenBright * ((255 - Projectile.alpha) / 255f);
        }
	}
}
