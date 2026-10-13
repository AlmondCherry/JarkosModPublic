using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class Fulgur_shot_Holdout : jarkos.projectiles.mecha.BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Fulgur_shot";
        public override float MuzzleForwardOffset => 105f;
        public override float MuzzleVerticalOffset => -4f;
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 12f;
        public override Vector2 HoldoutOrigin => new Vector2(42f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 30 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            recoil = 6f;
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/standart_mechashot") with { Pitch = Main.rand.NextFloat(-0.35f, 0.35f) }, Projectile.position);
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(MathHelper.ToRadians(25)) * Main.rand.NextFloat(0.3f, 0.8f);
                Dust dust = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Electric, dustVel.X, dustVel.Y);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
            }
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 projVel = velocity.RotatedByRandom(MathHelper.ToRadians(5)) * speed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projVel, ModContent.ProjectileType<Fulgur_shot_proj>(), damage, knockback, Projectile.owner);
            }
        }
    }
}
