using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public class Turgravir_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Turgravir";
        public override float MuzzleForwardOffset => 30f;
        public override float MuzzleVerticalOffset => -6f;
        public override float BaseRecoil => 8f;
        public override int FirstShotDelay => 30;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 1.0f, Pitch = 0.1f }, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                dustVel = dustVel.RotatedByRandom(MathHelper.ToRadians(20f));
                Dust dust = Dust.NewDustPerfect(muzzlePos, DustID.GoldFlame, dustVel, 0, default, 1.2f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                projToShoot = ModContent.ProjectileType<Turgravir_special_proj>();
                damage = (int)(damage * 1.5f);
            }
            Vector2 shotVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(3f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel, projToShoot, damage, knockback, Projectile.owner);
        }
    }
}
