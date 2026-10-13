using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public class Reflector_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Reflector";
        public override float MuzzleForwardOffset => 38f;
        public override float MuzzleVerticalOffset => -4f;
        public override float BaseRecoil => 8f;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item157, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(25)) * Main.rand.NextFloat(0.3f, 0.8f);
                Dust dust = Dust.NewDustPerfect(muzzlePos, DustID.Electric, dustVel, 0, default, 1.2f);
                dust.noGravity = true;
            }
            Vector2 shotVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(3f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel, ModContent.ProjectileType<Reflector_beam>(), damage, knockback, Projectile.owner);
        }
    }
}
