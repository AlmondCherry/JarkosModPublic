using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Atom_gun_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Atom_gun";
        public override float MuzzleForwardOffset => 134f;
        public override float MuzzleVerticalOffset => -8f;
        public override Vector2 HoldoutOrigin => new Vector2(36f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 2.2f;
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 7 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/AtomicGunShot") with { Volume = 0.70f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.5f, 3.5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, new Color(0, 255, 100), Main.rand.NextFloat(0.7f, 1.1f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust Spark = Dust.NewDustPerfect(muzzlePos, DustID.YellowTorch, sparkVel, 0, default, Main.rand.NextFloat(1.2f, 1.7f));
                Spark.noGravity = true;
                Spark.color = new Color(0, 255, 120);
            }
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Atom_proj>(), damage, knockback, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (recoil > 0.2f)
            {
                shakeOffset = Main.rand.NextVector2Circular(0.8f, 0.8f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
