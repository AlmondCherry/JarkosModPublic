using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
using jarkos.projectiles.bullet;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
    public class Jupiter_storm_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Jupiter_storm";
        public override Vector2 HoldoutOrigin => new Vector2(8f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float MuzzleForwardOffset => 30f;
        public override float MuzzleVerticalOffset => -4f;
        public override float BaseRecoil => 8f;
        public override int FirstShotDelay => 30;
        public override bool ConsumeAmmo()
        {
            if (Main.rand.NextFloat() < 0.80f) return true;
            return false;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            if (projToShoot == ProjectileID.Bullet)
            {
                projToShoot = ModContent.ProjectileType<Jupiter_water_bullet>();
            }
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.position);
            for (int i = 0; i < 3; i++)
            {
                Dust waterDust = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Water, Projectile.velocity.X * Main.rand.NextFloat(2f, 5f), Projectile.velocity.Y * Main.rand.NextFloat(2f, 5f), 100, default, 1.5f);
                waterDust.velocity += Projectile.velocity * 2f;
                waterDust.noGravity = true;
            }
            Vector2 shotVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(3f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel, projToShoot, damage, knockback, Projectile.owner);
            if (Main.rand.NextFloat() < 0.15f)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel * 0.4f, ModContent.ProjectileType<Jupiter_water_bomb>(), (int)(damage * 2.5f), knockback * 2f, Projectile.owner);
            }
        }
    }
}
