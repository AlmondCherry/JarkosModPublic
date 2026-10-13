using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.projectiles.mecha;
using Microsoft.Xna.Framework.Graphics;
namespace Jarkos.projectiles.mecha
{
    public class Threebarrel_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Threebarrel_atomgun";
        public override float MuzzleForwardOffset => 145f;
        public override float MuzzleVerticalOffset => -12f;
        public override Vector2 HoldoutOrigin => new Vector2(80f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        private int burstCount = 0;
        private int burstTimer = 0;
        public override int GetUseTime(Player player) => 9999;
        public override bool ShouldFire(Player player) => false;
        public override void UpdateExtra(Player player)
        {
            int cooldownTime = player.HeldItem.useTime;
            if (cooldownTime <= 0) cooldownTime = 12;
            if (shootTimer >= cooldownTime)
            {
                burstTimer++;
                if (burstTimer >= 4)
                {
                    burstTimer = 0;
                    burstCount++;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                        SoundEngine.PlaySound(SoundID.Item91 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.position);
                        recoil = 5f;
                        int damage = player.GetWeaponDamage(player.HeldItem);
                        float knockback = player.HeldItem.knockBack;
                        Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * player.HeldItem.shootSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Atomic_three_proj>(), damage, knockback, Projectile.owner);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                            new Spark(muzzlePos, sparkVel, new Color(100, 255, 100), 14, Main.rand.NextFloat(0.8f, 1.3f), 1f).Spawn();
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
