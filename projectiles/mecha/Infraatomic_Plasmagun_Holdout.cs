using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Infraatomic_Plasmagun_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Infraatomic_Plasmagun";
        public override float MuzzleForwardOffset => 88f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(36f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 8f;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.8f, 1.4f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 8f);
                Dust Spark = Dust.NewDustPerfect(muzzlePos, DustID.Torch, sparkVel, 0, new Color(255, 150, 50), Main.rand.NextFloat(1.1f, 1.6f));
                Spark.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.8f }, Projectile.position);
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Infraatomic_proj>(), damage, knockback, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (recoil > 0.5f)
            {
                shakeOffset = Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
