using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.ranged.firearm
{
    public class Sniper_horseman_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Sniper_horseman";
        public override float MuzzleForwardOffset => 80f;
        public override float MuzzleVerticalOffset => -5f;
        public override float BaseRecoil => 10f;
        public override int FirstShotDelay => 30;
        public override Vector2 HoldoutOrigin => new Vector2(20f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 1.2f, Pitch = -0.1f }, Projectile.position);
            for (int i = 0; i < 25; i++)
            {
                Dust smoke = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Smoke, Projectile.velocity.X * Main.rand.NextFloat(2f, 8f), Projectile.velocity.Y * Main.rand.NextFloat(2f, 8f), 100, default, Main.rand.NextFloat(1.5f, 3f));
                smoke.velocity += Projectile.velocity * 3f;
                smoke.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust Spark = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Torch, Projectile.velocity.X * Main.rand.NextFloat(4f, 10f), Projectile.velocity.Y * Main.rand.NextFloat(4f, 10f), 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                Spark.noGravity = true;
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * speed, projToShoot, damage, knockback, Projectile.owner);
        }
    }
}
