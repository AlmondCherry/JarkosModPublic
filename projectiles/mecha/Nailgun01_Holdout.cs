using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
using Microsoft.Xna.Framework.Graphics;
namespace Jarkos.projectiles.mecha
{
    public class Nailgun01_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Nailgun01";
        public override float MuzzleForwardOffset => 135f;
        public override float MuzzleVerticalOffset => -2f;
        public override Vector2 HoldoutOrigin => new Vector2(10f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        private int burstCount = 0;
        private int burstTimer = 0;
        public override int GetUseTime(Player player) => 9999;
        public override bool ShouldFire(Player player) => false;
        public override void UpdateExtra(Player player)
        {
            int cooldownTime = player.HeldItem.useTime;
            if (cooldownTime <= 0) cooldownTime = 20;
            if (shootTimer >= cooldownTime)
            {
                burstTimer++;
                if (burstTimer >= 5)
                {
                    burstTimer = 0;
                    burstCount++;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                        SoundEngine.PlaySound(SoundID.Item11 with { Pitch = 0.2f, Volume = 0.8f }, Projectile.position);
                        recoil = 6f;
                        int damage = player.GetWeaponDamage(player.HeldItem);
                        float knockback = player.HeldItem.knockBack;
                        Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(4f)) * player.HeldItem.shootSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Nail01_proj_charged>(), damage, knockback, Projectile.owner);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 6f);
                            Dust Spark = Dust.NewDustPerfect(muzzlePos, DustID.Torch, sparkVel, 100, default, Main.rand.NextFloat(1f, 1.5f));
                            Spark.noGravity = true;
                        }
                    }
                    if (burstCount >= 3)
                    {
                        burstCount = 0;
                        shootTimer = 0;
                    }
                }
            }
        }
    }
}
