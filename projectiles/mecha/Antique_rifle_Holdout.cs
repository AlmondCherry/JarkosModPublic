using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
namespace jarkos.projectiles.mecha
{
    public class Antique_rifle_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Antique_rifle";
        public override float MuzzleForwardOffset => 50f;
        public override float MuzzleVerticalOffset => -4f;
        public override Vector2 HoldoutOrigin => new Vector2(20f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        private int shotCounter = 0;
        public override int GetUseTime(Player player)
        {
            return shotCounter >= 5 ? 40 : 4;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            shotCounter++;
            if (shotCounter >= 5)
            {
                SoundEngine.PlaySound(SoundID.Item38, Projectile.position);
                recoil = 20f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, velocity * 12f, ModContent.ProjectileType<Antique_shell>(), damage * 5, knockback * 2f, player.whoAmI);
                for (int i = 0; i < 15; i++)
                {
                    Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Torch, velocity.X * Main.rand.NextFloat(0.5f, 2f), velocity.Y * Main.rand.NextFloat(0.5f, 2f), 0, default, 2f);
                    d.noGravity = true;
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item41, Projectile.position);
                recoil = 6f;
                float spread = MathHelper.ToRadians(6f);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 perturbedVel = velocity.RotatedByRandom(spread);
                    perturbedVel *= Main.rand.NextFloat(0.85f, 1.15f) * speed;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, perturbedVel, projToShoot, damage, knockback, player.whoAmI);
                }
                for (int i = 0; i < 5; i++)
                {
                    Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Smoke, velocity.X * Main.rand.NextFloat(0.2f, 1f), velocity.Y * Main.rand.NextFloat(0.2f, 1f), 100, default, 1.5f);
                    d.noGravity = true;
                }
            }
        }
        public override void AI()
        {
            base.AI();
            if (shotCounter >= 5 && shootTimer > 5 && shootTimer < 35)
            {
                Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                if (Main.rand.NextBool(2))
                {
                    Vector2 steamVel = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(1f, 4f);
                    Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Cloud, steamVel.X, steamVel.Y, 100, default, Main.rand.NextFloat(1f, 2f));
                    d.noGravity = true;
                }
            }
            if (shootTimer == 0 && shotCounter >= 5)
            {
                shotCounter = 0;
            }
        }
        public override void PostDrawHoldout(Vector2 drawPos, float rotation, SpriteEffects spriteEffects)
        {
            float heatPercent = shotCounter / 5f;
            if (heatPercent > 0)
            {
                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                Main.EntitySpriteDraw(texture, drawPos, null, new Color(255, 50, 0, 0) * (heatPercent * 0.8f), rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0f);
            }
        }
    }
}
