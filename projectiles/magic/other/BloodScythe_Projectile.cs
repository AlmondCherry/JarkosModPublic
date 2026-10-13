using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.magic.other
{
    public class BloodScythe_Projectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.4f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        private float timer = 0f;
        public override void AI()
        {
            timer++;

            float spinDir = Projectile.ai[0];
            Projectile.rotation += 0.35f * spinDir;
            Projectile.velocity *= 0.985f;
            Projectile.velocity = Projectile.velocity.RotatedBy(0.015f * spinDir);

            float pulse = (float)Math.Sin(timer * 0.15f) * 0.2f + 0.6f;

            Lighting.AddLight(Projectile.Center, 0.7f * pulse, 0.08f * pulse, 0.12f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 0, 0, DustID.Blood, 0, 0, 100, default, 0.7f);
                d.noGravity = true;
                d.velocity *= 0.15f;
            }

            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.LifeDrain, 0, 0, 120, new Color(140, 15, 15), 0.5f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }

            if (Projectile.timeLeft < 30)
                Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - (Projectile.timeLeft / 30f));
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            int heal = Math.Max(1, damageDone / 10);

            if (owner.statLife < owner.statLifeMax2)
                owner.Heal(heal);

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1f, 1.6f));
                d.noGravity = Main.rand.NextBool(3);
            }

            if (heal > 0)
                CombatText.NewText(owner.Hitbox, new Color(100, 255, 100), $"+{heal}", false, false);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D px = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;

            float alpha = 1f - (Projectile.alpha / 255f);

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
                float fade = (1f - p) * alpha;

                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(70, 3, 8, 0) * fade * 0.35f, rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(14f, 2f, p)), SpriteEffects.None, 0f);
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(200, 20, 30, 0) * fade * 0.45f, rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(7f, 1f, p)), SpriteEffects.None, 0f);
                sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), new Color(255, 130, 120, 0) * fade * 0.5f, rot, new Vector2(0, 0.5f), new Vector2(len, MathHelper.Lerp(3f, 0.3f, p)), SpriteEffects.None, 0f);
            }

            float bladeGlow = (float)Math.Sin(timer * 0.2f) * 0.2f + 0.8f;

            for (int i = 0; i < 4; i++)
            {
                float angle = Projectile.rotation + i * MathHelper.PiOver2;
                float bladeLen = 16f * alpha;

                sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(150, 15, 20, 0) * 0.6f * alpha, angle, new Vector2(0, 0.5f), new Vector2(bladeLen, 3f), SpriteEffects.None, 0f);
                sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(255, 100, 90, 0) * 0.4f * alpha, angle, new Vector2(0, 0.5f), new Vector2(bladeLen * 0.8f, 1.5f), SpriteEffects.None, 0f);
            }
            
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(120, 8, 12, 0) * 0.2f * bladeGlow * alpha, 0f, new Vector2(0.5f), new Vector2(30f) * bladeGlow, SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(220, 30, 30, 0) * 0.4f * alpha, 0f, new Vector2(0.5f), new Vector2(12f), SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(255, 180, 170, 0) * 0.7f * alpha, 0f, new Vector2(0.5f), new Vector2(5f), SpriteEffects.None, 0f);
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), new Color(255, 235, 230, 0) * 0.9f * alpha, 0f, new Vector2(0.5f), new Vector2(2.5f), SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = Main.rand.NextBool(3);
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.LifeDrain, vel.X, vel.Y, 0, new Color(150, 15, 15), 1.1f);
                d.noGravity = true;
            }
        }
    }
}
