using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.magic.other
{
	public class Magicprojvtwo : ModProjectile
	{
		public override void SetStaticDefaults() 
        {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() 
        {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
		}
        public override void AI()
        {
            Projectile.velocity *= 0.975f;
        }
        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
		public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float width_trail = 12f;
            float baseWidth = Projectile.scale * (Projectile.width / 4) * width_trail;

            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Green, Color.Transparent, completionRatio) * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            TrailDrawer ??= new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
            GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(Trailreg.test);
            TrailDrawer.DrawPrims(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 155);
            return false;
        }
    }
}
