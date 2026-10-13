using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Diffusal_shot_Holdout : BaseRangedHoldout
    {
        public override string Texture => "Jarkos/items/weapons/ranged/firing/Diffusal_shot";
        public override float MuzzleForwardOffset => 125f;
        public override float MuzzleVerticalOffset => -8f;
        public override Vector2 HoldoutOrigin => new Vector2(40f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 8.5f;
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 18f;
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 24 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.5f, Pitch = -0.3f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.position);
            for (int i = 0; i < 8; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Cyan, Main.rand.NextFloat(0.7f, 1.2f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 electricVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f);
                Dust electric = Dust.NewDustPerfect(muzzlePos, DustID.Electric, electricVel, 0, default, Main.rand.NextFloat(0.3f, 0.6f));
                electric.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 8f);
                new Spark(muzzlePos, sparkVel, new Color(0, 230, 255), 16, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 8f);
                new Spark(muzzlePos, sparkVel, new Color(255, 30, 220), 16, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(1.5f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Diffusal_shot_proj>(), damage, knockback, Projectile.owner);
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
