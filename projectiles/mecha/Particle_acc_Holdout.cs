using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.projectiles.mecha;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
    public class Particle_acc_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Particle_acc";
        public override float MuzzleForwardOffset => 115f;
        public override float MuzzleVerticalOffset => -3f;
        public override float BaseRecoil => 6f;
        public override int FirstShotDelay => 20;
        public override Vector2 HoldoutOrigin => new Vector2(42f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override bool ConsumeAmmo() => false;
        private int shotCounter = 0;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            shotCounter++;
            if (shotCounter % 3 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.4f, Pitch = 0.8f + Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
            }
            else if (shotCounter % 5 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item75 with { Volume = 0.3f, Pitch = 1.2f }, Projectile.position);
            }
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.25f, Pitch = 1.5f + Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.YellowStarfish, 0f, 0f, 100, default, 0.6f);
                dust.velocity = Projectile.velocity * speed * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                dust.noGravity = true;
            }
            Vector2 shootVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2.5f)) * speed;
            if (shotCounter % 10 == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shootVel * 1.2f, ModContent.ProjectileType<ChargedParticle>(), (int)(damage * 1.5f), knockback * 2f, player.whoAmI);
                SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.5f, Pitch = 0.6f }, Projectile.position);
                recoil = 12f;
            }
            else
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shootVel, ModContent.ProjectileType<Particle_acc_proj>(), damage, knockback, player.whoAmI);
                recoil = 6f;
            }
        }
    }
}
