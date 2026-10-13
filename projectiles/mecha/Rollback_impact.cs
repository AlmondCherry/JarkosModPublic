using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
namespace jarkos.projectiles.mecha
{
	public class Rollback_impact : ModProjectile
	{
        private float expansionProgress = 0f;
        private const float MaxRadius = 480f;
        public override void SetDefaults()
        {
            Projectile.width = (int)(MaxRadius * 2);
            Projectile.height = (int)(MaxRadius * 2);
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            expansionProgress += 1f / 15f;
            Projectile.ai[0] = expansionProgress;
            float currentRadius = MaxRadius * expansionProgress;
            float intensity = 1.5f * (1f - expansionProgress);
            Lighting.AddLight(Projectile.Center, 0.5f * intensity, 0.7f * intensity, 1.5f * intensity);
            if (expansionProgress < 0.8f)
            {
                int particleCount = 8;
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi / particleCount * i + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius;
                    Vector2 pos = Projectile.Center + offset;
                    Dust dust = Dust.NewDustPerfect(pos, DustID.OrangeTorch, offset.SafeNormalize(Vector2.Zero) * 2f, 0, default, 0.8f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.5f;
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float currentRadius = MaxRadius * expansionProgress;
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            return distance < currentRadius;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float distance = Vector2.Distance(Projectile.Center, target.Center);
            float damageFalloff = 1f - distance / MaxRadius * 0.5f;
            modifiers.FinalDamage *= damageFalloff;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.OrangeTorch, 0, 0, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float currentRadius = MaxRadius * expansionProgress;
            float alpha = 1f - expansionProgress;
            int segments = 64;
            float ringWidth = 8f * (1f - expansionProgress * 0.5f);
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi / segments * i;
                float angle2 = MathHelper.TwoPi / segments * (i + 1);
                Vector2 pos1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * currentRadius;
                Vector2 pos2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * currentRadius;
                Vector2 direction = pos2 - pos1;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Color outerColor = new Color(250, 255, 80) * alpha * 0.5f;
                Rectangle outerRect = new Rectangle(0, 0, (int)Math.Max(length + 2, 1), (int)(ringWidth * 1.5f));
                spriteBatch.Draw(pixel, pos1, outerRect, outerColor, rotation, new Vector2(0, ringWidth * 0.75f), 1f, SpriteEffects.None, 0f);
                Color innerColor = new Color(250, 255, 80) * alpha * 0.8f;
                Rectangle innerRect = new Rectangle(0, 0, (int)Math.Max(length + 2, 1), (int)ringWidth);
                spriteBatch.Draw(pixel, pos1, innerRect, innerColor, rotation, new Vector2(0, ringWidth / 2f), 1f, SpriteEffects.None, 0f);
                Color coreColor = Color.White * alpha;
                Rectangle coreRect = new Rectangle(0, 0, (int)Math.Max(length + 2, 1), (int)(ringWidth * 0.3f));
                spriteBatch.Draw(pixel, pos1, coreRect, coreColor, rotation, new Vector2(0, ringWidth * 0.15f), 1f, SpriteEffects.None, 0f);
            }
            float flashSize = 40f * (1f - expansionProgress);
            Color flashColor = Color.White * alpha * 0.5f;
            Rectangle flashRect = new Rectangle(0, 0, (int)flashSize, (int)flashSize);
            spriteBatch.Draw(pixel, center, flashRect, flashColor, 0f, new Vector2(flashSize / 2f), 1f, SpriteEffects.None, 0f);
            for (int glow = 0; glow < 3; glow++)
            {
                float glowRadius = currentRadius * (0.3f + glow * 0.2f);
                float glowAlpha = alpha * (0.15f - glow * 0.04f);
                Color glowColor = new Color(100, 180, 255) * glowAlpha;
                float size = glowRadius * 2;
                Rectangle glowRect = new Rectangle(0, 0, (int)size, (int)size);
                spriteBatch.Draw(pixel, center, glowRect, glowColor, 0f, new Vector2(size / 2f), 1f, SpriteEffects.None, 0f);
            }
            return false;
        }
	}
}
