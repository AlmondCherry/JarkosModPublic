using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Jarkos.trails;
using Terraria.Graphics.Shaders;
using jarkos.particles;
namespace jarkos.projectiles.enemy.tenderote
{
	public class Am05_smaller : ModProjectile
	{
        private float speedtimer = 0;
        public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 175;
			Projectile.penetrate = 1;
			Projectile.light = 5f;
		}
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, 0f, 1f);
			Player player = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity += (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            speedtimer++;
			if (speedtimer == 10) {
			Projectile.velocity /= 4f;
			speedtimer = 0;
			}
        }
        public override void OnKill(int timeLeft)
        {
            for(int i = 0; i < 10; i++)
            {
            Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
            float size = Main.rand.NextFloat(0.2f, 0.5f);
            new Dust1_particle(Projectile.Center, vct, Color.Blue, 60, size, 1f, 0f).Spawn();
            new Circular_glow(Projectile.Center, vct, Color.Cyan, 80, 0.6f, 1f, 0f).Spawn();
            }
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
        public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float width_trail = 3f;
            float baseWidth = Projectile.scale * (Projectile.width / 4) * width_trail;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Blue, Color.Transparent, completionRatio) * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            TrailDrawer ??= new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
            GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(Trailreg.test);
            TrailDrawer.DrawPrims(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 155);
            return true;
        }
    }
}
