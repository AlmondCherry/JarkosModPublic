using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.Audio;
namespace jarkos.projectiles.magic.other
{
	public class Blood_sp : ModProjectile
	{
		public override void SetStaticDefaults() 
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.damage = 1;
            Projectile.timeLeft = 600;
		}
        public override void AI()
		{
			if (Main.rand.NextBool(1, 1)) 
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Blood, Scale: 1f);
				dust.velocity /= 1;
				dust.noGravity = true; 
			}

            if (Main.rand.NextBool(1, 1)) 
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Blood, Scale: 1f);
				dust.velocity /= 1;
				dust.noGravity = true;
			}
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 15; i++) 
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(4f, 4f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Blood, speed * 2.5f, Scale: 1.5f);
				d.noGravity = true;
        	}

			Player player = Main.player[Projectile.owner];
			player.Heal(5);
        }
		public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        private static VertexStrip _vertexStrip = new VertexStrip();
          public override bool PreDraw(ref Color lightcolor) {
            Draw(Projectile);
            return true;
        }
	    public void Draw(Projectile proj)
	    {
		    MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();
		    Blood_sp._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Blood_sp._vertexStrip.DrawTrail();
		    Terraria.Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
            Color result = Color.Red;
		    return result;
	    }
	    private float StripWidth(float progressOnStrip)
	    {
		    float num = 1f;
		    float lerpValue = Terraria.Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
		    num *= 1f - (1f - lerpValue) * (1f - lerpValue);
		    return MathHelper.Lerp(0f, 8f, num);
	    }
	}
}
