using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace jarkos.projectiles.melee.other_melee
{
    public class Frozen_yoyo_shard : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/other_melee/frozenyoyo";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            Projectile.rotation += 0.18f;
            Projectile.velocity = Projectile.velocity.RotatedBy(0.015f);
            Lighting.AddLight(Projectile.Center, 0.1f, 0.4f, 0.8f);
            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
            {
                new Spark(Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat()), 15 + Main.rand.Next(10), 0.25f + Main.rand.NextFloat(0.15f), 0.7f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 300);
            if (Main.netMode == NetmodeID.Server)
                return;
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(2f, 2f) - Projectile.velocity * 0.1f;
                new Spark(Projectile.Center, sparkVel, Color.Lerp(Color.Cyan, Color.DeepSkyBlue, Main.rand.NextFloat()), 20, 0.3f, 0.8f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                new Spark(Projectile.Center, vel, Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat()), 25, 0.35f, 0.9f).Spawn();
            }
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 currentPos = Projectile.oldPos[i - 1] + Projectile.Size / 2f;
                Vector2 previousPos = Projectile.oldPos[i] + Projectile.Size / 2f;
                if (Projectile.oldPos[i - 1] == Vector2.Zero || Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                Vector2 dir = currentPos - previousPos;
                float length = dir.Length();
                if (length < 0.5f) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.7f;
                float width = 6f * (1f - progress);
                Color outerColor = Color.DeepSkyBlue * alpha * 0.3f;
                Color innerColor = Color.Cyan * alpha * 0.8f;
                Color coreColor = Color.White * alpha * 0.9f;
                Vector2 drawPos = previousPos - Main.screenPosition;
                float rot = dir.ToRotation();
                Vector2 origin = new Vector2(0f, 0.5f);
                sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), outerColor, rot, origin, new Vector2(length + 1f, width * 3f), SpriteEffects.None, 0f);
                sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), innerColor, rot, origin, new Vector2(length + 1f, width * 1.5f), SpriteEffects.None, 0f);
                sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), coreColor, rot, origin, new Vector2(length + 1f, width * 0.5f), SpriteEffects.None, 0f);
            }
            Vector2 center = Projectile.Center - Main.screenPosition;
            for (int j = 0; j < 2; j++)
            {
                float angle = Projectile.rotation + j * MathHelper.PiOver2;
                sb.Draw(pixel, center, new Rectangle(0, 0, 1, 1), Color.Cyan * 0.9f, angle, new Vector2(0.5f, 0.5f), new Vector2(18f, 3.5f), SpriteEffects.None, 0f);
                sb.Draw(pixel, center, new Rectangle(0, 0, 1, 1), Color.White * 0.95f, angle, new Vector2(0.5f, 0.5f), new Vector2(14f, 1.2f), SpriteEffects.None, 0f);
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            if (texture != null)
            {
                sb.Draw(texture, center, null, Color.LightBlue * 0.8f, -Projectile.rotation * 0.5f, texture.Size() / 2f, Projectile.scale * 0.45f, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
