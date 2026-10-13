using jarkos.particles;
using jarkos.systems;
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
	public class Meteoritegun_proj : ModProjectile
	{
		float direction = Main.rand.NextFloat(-1f,1f);
		public override void SetStaticDefaults() 
        {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() 
        {
            Projectile.width = 15;
      		Projectile.height = 15;
      		Projectile.friendly = true;
      		Projectile.penetrate = 1;
      		Projectile.DamageType = DamageClass.Magic;
      		Projectile.timeLeft = 300;
      		Projectile.usesLocalNPCImmunity = true;
      		Projectile.localNPCHitCooldown = 16;
            Projectile.light = 1f;
		}
        public override void AI()
        {
        	Projectile.velocity.Y = Projectile.velocity.Y + (0.2293803f * direction);
        	Projectile.rotation = - 1.5f;
            Projectile.spriteDirection = 0;

			NPChoming.HomingProj(Projectile, 2000f, 3f, 300);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<Meteorite_explosion>(), 30, 0f, Projectile.owner, 0f, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.scale * (Projectile.width / 4) * 6.3f;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Yellow, Color.Transparent, completionRatio) * 0.7f;
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
