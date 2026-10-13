using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.other
{
	public class VoidPlasma : ModProjectile
	{
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 160;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.3f;
        }
        private float timer = 0f;
        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = (float)Math.Sin(timer * 0.2f) * 0.2f + 0.5f;
            Lighting.AddLight(Projectile.Center, 0.5f * pulse, 0.15f * pulse, 0.7f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, 0, 0, 120, new Color(140, 40, 200), 0.5f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.05f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust dark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Shadowflame, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f), 150, default, 0.4f);
                dark.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D px = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float p = (float)i / Projectile.oldPos.Length;
                Vector2 p1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 p2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 dir = p2 - p1;
                float len = dir.Length();
                if (len == 0) continue;
                float rot = dir.ToRotation();
                float fade = 1f - p;
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(60, 10, 100, 0) * fade * 0.35f, rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(9f, 2f, p)), SpriteEffects.None, 0f);
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(160, 50, 230, 0) * fade * 0.45f, rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(5f, 1f, p)), SpriteEffects.None, 0f);
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(220, 180, 255, 0) * fade * 0.55f, rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(2f, 0.3f, p)), SpriteEffects.None, 0f);
            }
            float glow = (float)Math.Sin(timer * 0.17f) * 0.25f + 0.7f;
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(80, 20, 140, 0) * 0.2f * glow, 0f, new Vector2(0.5f), new Vector2(24f) * glow, SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(160, 60, 230, 0) * 0.4f * glow, 0f, new Vector2(0.5f), new Vector2(12f) * glow, SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(230, 200, 255, 0) * 0.8f, 0f, new Vector2(0.5f), new Vector2(4f), SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, vel.X, vel.Y, 0, new Color(160, 50, 230), 1.1f);
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 120);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Shadowflame, vel.X, vel.Y, 0, default, 1f);
                d.noGravity = true;
            }
        }
    }
}
