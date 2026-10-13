using System;
using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.bullet
{
    public class MiniShockwave : ModProjectile
    {
        private float currentRadius = 0f;
        private const float MaxRadius = 120f;
        private float expansionSpeed = 6f;
        private float waveAlpha = 1f;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            currentRadius += expansionSpeed;
            expansionSpeed *= 0.94f;
            waveAlpha = 1f - (currentRadius / MaxRadius);
            waveAlpha = Math.Max(waveAlpha, 0f);

            if (currentRadius >= MaxRadius || waveAlpha <= 0.01f)
            {
                Projectile.Kill();
                return;
            }
            if (waveAlpha > 0.15f)
            {
                int count = (int)(8 * waveAlpha);
                for (int i = 0; i < count; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle) * currentRadius, (float)Math.Sin(angle) * currentRadius);
                    Dust dust = Dust.NewDustDirect(pos, 2, 2, DustID.PurpleTorch, 0, 0, 150, default, 0.9f * waveAlpha);
                    dust.noGravity = true;
                    dust.velocity *= 0.1f;
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float closestX = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
            float closestY = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);

            float dist = Vector2.Distance(Projectile.Center, new Vector2(closestX, closestY));

            if (dist >= currentRadius - 20 && dist <= currentRadius + 10)
            {
                return true;
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int d = 0; d < 8; d++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.PurpleTorch, 0, 0, 120, default, 1.1f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(3, 3);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (currentRadius < 3f) return false;
            Main.spriteBatch.End();

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float scale = currentRadius * 2f / texture.Width;

            Color ringColor = new Color(255, 200, 50) * waveAlpha * 0.7f;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(texture, center, null, ringColor, 0f, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == (int)Projectile.ai[0])
            {
                return false;
            }
            return base.CanHitNPC(target);
        }
    }
    public class ResonanceGlobalNPC : GlobalNPC
    {
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<ResonanceDebuff>()))
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f + 0.7f;
                
                Color glowColor = new Color(255, 200, 50) * pulse * 0.3f;
                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Vector2 origin = new Vector2(texture.Width / 2, texture.Height / Main.npcFrameCount[npc.type] / 2);
                Rectangle frame = npc.frame;

                float offset = 2f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offsetVec = new Vector2(i < 2 ? -offset : offset, i % 2 == 0 ? -offset : offset);
                    spriteBatch.Draw(texture, npc.Center - screenPos + offsetVec, frame, glowColor, npc.rotation, origin, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }
}
