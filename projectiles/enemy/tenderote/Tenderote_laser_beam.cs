using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.trails;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using System;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_laser_beam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PhantasmalDeathray;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
        }
        private float LaserLength = 0f;
        private int Time = 0;
        public override void AI()
        {
            Projectile parent = Main.projectile[(int)Projectile.ai[0]];
            if (!parent.active || parent.type != ModContent.ProjectileType<Laser_sphere>())
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = parent.Center;
            Projectile.velocity = parent.rotation.ToRotationVector2();
            Time++;
            if (Time >= Projectile.timeLeft)
            {
                Projectile.Kill();
                return;
            }
            if (Time < 10)
                Projectile.scale = Time / 10f;
            else if (Projectile.timeLeft - Time < 10)
                Projectile.scale = (Projectile.timeLeft - Time) / 10f;
            else
                Projectile.scale = 1f;
            LaserLength = 3000f;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Time < 10) return false;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, 40f * Projectile.scale, ref collisionPoint);
        }
        private Primitivedraw TrailDrawer;
        public override void PostDraw(Color lightColor)
        {
            if (TrailDrawer == null)
            {
                TrailDrawer = new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
                GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
            }
            Vector2[] positions = new Vector2[10];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Projectile.Center + Projectile.velocity * (LaserLength * (i / (float)(positions.Length - 1)));
            }
            GameShaders.Misc["Jarkos:LaserShader"].UseColor(Color.White);
            GameShaders.Misc["Jarkos:LaserShader"].UseSecondaryColor(Color.Black);
            GameShaders.Misc["Jarkos:LaserShader"].UseShaderSpecificData(new Vector4(2f, 1f, 0, 0));
            TrailDrawer.DrawPrims(positions, -Main.screenPosition, 10);
        }
        private float WidthFunction(float completionRatio)
        {
            float baseWidth = 40f * Projectile.scale * Projectile.Opacity;
            return (float)Math.Sin(completionRatio * MathHelper.Pi) * baseWidth;
        }
        private Color ColorFunction(float completionRatio)
        {
            return Color.White;
        }
    }
}
