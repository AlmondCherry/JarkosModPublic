using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace jarkos.projectiles.ranged.firearm
{
	public class Celestian_shard_bullet : ModProjectile
	{
       public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        private Color shardColor;
        private bool initialized = false;
        public override void AI()
        {
            if (!initialized)
            {
                float hueOffset = Main.rand.NextFloat();
                shardColor = Color.Lerp(new Color(80, 180, 255), new Color(200, 80, 255), hueOffset);
                initialized = true;
                Projectile.ai[1] = 0;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.ai[0]++;
            Projectile.ai[1]++;
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.2f + 0.5f;
            Lighting.AddLight(Projectile.Center, shardColor.R / 255f * pulse, shardColor.G / 255f * pulse, shardColor.B / 255f * pulse);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueTorch, 0, 0, 0, shardColor, 0.6f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.05f;
            }
            if (Projectile.ai[1] > 15)
            {
                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                    float homingStrength = MathHelper.Clamp((Projectile.ai[1] - 15) / 30f, 0f, 1f) * 0.12f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, homingStrength);
                }
            }
            if (Projectile.velocity.Length() < 16f)
            {
                Projectile.velocity *= 1.01f;
            }
        }
        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color outerColor = shardColor * (1f - progress) * 0.3f;
                outerColor.A = 0;
                float outerWidth = MathHelper.Lerp(8f, 1f, progress);
                Color innerColor = Color.Lerp(shardColor, Color.White, 0.5f) * (1f - progress) * 0.6f;
                innerColor.A = 0;
                float innerWidth = MathHelper.Lerp(3f, 0.5f, progress);
                Vector2 pos1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 pos2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 dir = pos2 - pos1;
                float len = dir.Length();
                if (len == 0) continue;
                float rot = dir.ToRotation();
                spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), outerColor, rot, new Vector2(0, 0.5f), new Vector2(len, outerWidth), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), innerColor, rot, new Vector2(0, 0.5f), new Vector2(len, innerWidth), SpriteEffects.None, 0f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float glow = (float)Math.Sin(Projectile.ai[0] * 0.25f) * 0.3f + 0.7f;
            Color glowColor = shardColor;
            glowColor.A = 0;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), glowColor * 0.35f * glow, 0f, new Vector2(0.5f), new Vector2(16f) * glow, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(240, 245, 255, 0) * 0.8f, 0f, new Vector2(0.5f), new Vector2(5f), SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 0, shardColor, 1.2f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.PurpleTorch, vel.X, vel.Y, 0, default, 1.4f);
                dust.noGravity = true;
            }
        }
    }
}
