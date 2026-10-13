using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.other
{
    public class BloodLaserProjectile : ModProjectile
    {
        public const float MaxLaserLength = 2200f;
        public const float LaserWidth = 132f;
        public const int Lifetime = 60;
        public override string Texture => "Terraria/Images/Projectile_1";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.alpha = 255;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override void AI()
        {
            if (Projectile.velocity == Vector2.Zero)
                Projectile.velocity = -Vector2.UnitY;

            Projectile.velocity = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
            Projectile.rotation = Projectile.velocity.ToRotation();

            float progress = 1f - Projectile.timeLeft / (float)Lifetime;
            Projectile.Opacity = GetOpacity(progress);

            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.2f, 0.45f) * Projectile.Opacity);

            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                d.scale = 1.4f * Projectile.Opacity;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 direction = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
            Vector2 start = Projectile.Center;
            Vector2 end = start + direction * GetLaserLength(Projectile.Center, direction);
            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, LaserWidth * 0.32f, ref collisionPoint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;

        }
        public static float GetLaserLength(Vector2 start, Vector2 direction)
        {
            for (float distance = 24f; distance <= MaxLaserLength; distance += 24f)
            {
                Vector2 position = start + direction * distance;
                if (Collision.SolidCollision(position - new Vector2(8f), 16, 16))
                    return distance;
            }

            return MaxLaserLength;
        }
        private static float GetOpacity(float progress)
        {
            float fadeIn = MathHelper.Clamp(progress / 0.10f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((1f - progress) / 0.20f, 0f, 1f);
            return fadeIn * fadeOut;
        }
    }
}
