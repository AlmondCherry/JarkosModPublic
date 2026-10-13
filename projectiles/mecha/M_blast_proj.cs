using jarkos;
using jarkos.buffs.debuff;
using jarkos.particles;
using jarkos.particles.icestaff;
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
	public class M_blast_proj : ModProjectile
	{
		private static readonly Color FragmentGreen = new Color(80, 255, 120);
        private const int TrailLength = 12;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 45;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.alpha = 100;
        }
        public override void AI()
        {
            Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
            Projectile.velocity *= 0.96f;
            Projectile.alpha += 3;
            if (Projectile.alpha > 255)
                Projectile.Kill();
            float lightMult = (255 - Projectile.alpha) / 255f;
            Lighting.AddLight(Projectile.Center, 0.25f * lightMult, 0.6f * lightMult, 0.25f * lightMult);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 150, default, 0.7f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.2f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            float alphaFactor = (255 - Projectile.alpha) / 255f;
            Vector2[] positions = Projectile.oldPos;
            for (int i = 1; i < TrailLength; i++)
            {
                if (positions[i] == Vector2.Zero || positions[i - 1] == Vector2.Zero)
                    continue;
                Vector2 currentPos = positions[i] + Projectile.Size / 2f;
                Vector2 previousPos = positions[i - 1] + Projectile.Size / 2f;
                float progress = 1f - (float)i / TrailLength;
                float width = 5f * progress * alphaFactor;
                float alpha = progress * 0.7f * alphaFactor;
                Color trailColor = FragmentGreen * alpha;
                Vector2 direction = previousPos - currentPos;
                float length = direction.Length();
                if (length > 0)
                {
                    float rotation = direction.ToRotation() + MathHelper.PiOver2;
                    spriteBatch.Draw(pixel, currentPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0.5f, 0f), new Vector2(width, length), SpriteEffects.None, 0f);
                }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), FragmentGreen * 0.5f * alphaFactor, Projectile.rotation, new Vector2(0.5f), 12f * alphaFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White * alphaFactor, Projectile.rotation, new Vector2(0.5f), 5f * alphaFactor, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GreenTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.GreenTorch, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f), 100, default, 0.8f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return FragmentGreen * ((255 - Projectile.alpha) / 255f);
        }
	}
}
