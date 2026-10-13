using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using jarkos.buffs.debuff;
using Terraria.Audio;
namespace jarkos.projectiles.bullet
{
	public class Anticvararrow : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 8;
			Projectile.height = 20;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
		}
        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.damage += 25;
			for (int i = 0; i < 75; i++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.YellowTorch, speed * 2.5f, Scale: 3.5f);
			d.noGravity = true;
        	}
            target.AddBuff(ModContent.BuffType<Ancient_venom_buff>(), 240);
        }

		public override void OnKill(int timeLeft)
        {
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
		public override bool PreDraw(ref Color lightcolor) {
			Draw(Projectile);
			return true;
        }
        private static VertexStrip _vertexStrip = new VertexStrip();
	    public void Draw(Projectile proj)
	    {
		    MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();
		    Anticvararrow._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Anticvararrow._vertexStrip.DrawTrail();
		    Terraria.Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
           	Color result = Color.Lerp(Color.OrangeRed, Color.Transparent, (progressOnStrip - Main.GlobalTimeWrappedHourly)) * 0.7f;
		    return result;
	    }
	    private float StripWidth(float progressOnStrip)
	    {
		    float num = 1f;
		    float lerpValue = Terraria.Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
		    num *= 1f - (1f - lerpValue) * (1f - lerpValue);
		    return MathHelper.Lerp(0f, 32f, num);
	    }
    }
}
