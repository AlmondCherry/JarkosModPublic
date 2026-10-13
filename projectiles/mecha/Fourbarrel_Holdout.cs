using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace Jarkos.projectiles.mecha
{
    public class Fourbarrel_Holdout : jarkos.projectiles.mecha.BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Fourbarrel_gun";
        public override float MuzzleForwardOffset => 145f;
        public override float MuzzleVerticalOffset => -11f;
        public override Vector2 HoldoutOrigin => new Vector2(40f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 3f;
        public override float RecoilDecay => 0.8f;
        public override float CustomShootSpeed => 12f;
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 6 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(3f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Fourbarrel_main_proj>(), damage, knockback, Projectile.owner);
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 6f);
                new Spark(muzzlePos, sparkVel, new Color(100, 255, 100), 12, Main.rand.NextFloat(0.8f, 1.2f), 1f).Spawn();
            }
        }
    }
}
