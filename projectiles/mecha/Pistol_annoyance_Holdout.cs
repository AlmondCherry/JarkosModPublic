using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
using jarkos.projectiles.ranged.firearm;
namespace jarkos.items.weapons.ranged.firing
{
    public class Pistol_annoyance_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Pistol_annoyance";
        public override float MuzzleForwardOffset => 38f;
        public override float MuzzleVerticalOffset => -5f;
        public override float BaseRecoil => 6f;
        public override int FirstShotDelay => 30;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item11 with
            {
                Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.1f, 0.15f)
            }, muzzlePos);
            SpawnMuzzleFlash(muzzlePos, Projectile.velocity);
            Vector2 shotVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2.5f));
            shotVel *= Main.rand.NextFloat(0.95f, 1.05f) * speed;
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel, ModContent.ProjectileType<Pistol_annoyance_proj>(), damage, knockback, Projectile.owner);
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
            }
        }
        private void SpawnMuzzleFlash(Vector2 pos, Vector2 dir)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = dir * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Blood, vel.X, vel.Y, 80, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            Dust Spark = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch, dir.X * 3f, dir.Y * 3f, 0, default, 0.8f);
            Spark.noGravity = true;
        }
    }
}
