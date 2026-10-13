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
    public class Dark_matter_gun_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Dark_matter_gun";
        public override float MuzzleForwardOffset => 98f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(28f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            if (useTime <= 0) useTime = 5;
            return (int)(useTime / 0.43f);
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            bool heavyShot = Main.rand.NextBool(5);
            if (heavyShot)
            {
                recoil = 13f;
                SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.9f, Pitch = -0.1f }, Projectile.position);
                for (int i = 0; i < 9; i++)
                {
                    Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 7f);
                    Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.9f, 1.5f));
                    smoke.noGravity = true;
                }
                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.7f) * Main.rand.NextFloat(5f, 11f);
                    new Spark(muzzlePos, sparkVel, new Color(100, 200, 255), 22, Main.rand.NextFloat(1.0f, 1.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                }
                float spreadAngle = MathHelper.ToRadians(8f);
                for (int i = 0; i < 3; i++)
                {
                    Vector2 perturbedVelocity = Projectile.velocity.RotatedByRandom(spreadAngle) * speed * (1f - Main.rand.NextFloat(0.1f));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, perturbedVelocity, ModContent.ProjectileType<Dark_matter_proj>(), damage, knockback, Projectile.owner);
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * speed, ModContent.ProjectileType<Dark_matter_sine_proj>(), (int)(damage * 1.25f), knockback * 1.5f, Projectile.owner, 0f, 1f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * speed, ModContent.ProjectileType<Dark_matter_sine_proj>(), (int)(damage * 1.25f), knockback * 1.5f, Projectile.owner, 0f, -1f);
            }
            else
            {
                recoil = 6.5f;
                SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.75f }, Projectile.position);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                    Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.8f, 1.3f));
                    smoke.noGravity = true;
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 8f);
                    new Spark(muzzlePos, sparkVel, new Color(100, 200, 255), 16, Main.rand.NextFloat(0.8f, 1.3f), 1f, 0f).Spawn();
                }
                float spreadAngle = MathHelper.ToRadians(8f);
                for (int i = 0; i < 3; i++)
                {
                    Vector2 perturbedVelocity = Projectile.velocity.RotatedByRandom(spreadAngle) * speed * (1f - Main.rand.NextFloat(0.1f));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, perturbedVelocity, ModContent.ProjectileType<Dark_matter_proj>(), damage, knockback, Projectile.owner);
                }
            }
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
