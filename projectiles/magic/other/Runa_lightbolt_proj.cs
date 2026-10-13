using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using jarkos.systems;
using jarkos.projectiles.bullet;

namespace jarkos.projectiles.magic.other
{
	public class Runa_lightbolt_proj : ModProjectile
	{
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 300;
			Projectile.light = 1f;
			DrawOffsetX = -(int)((28 / 2) - (Projectile.Size.X / 2));
			DrawOriginOffsetY = -(int)((28 / 2) - (Projectile.Size.Y / 2));
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			Vector2 start = Projectile.position += new Vector2(-3f, -200f);
			Vector2 end = Projectile.position += new Vector2(3f, 225f);
			Vector2 start2 = Projectile.Center += new Vector2(-3f, -200f) * 3;
			Vector2 end2 = Projectile.Center += new Vector2(3f, 225f) * 3;
			LightningSystemLaser.MakeProjectileLighting(start2, end2, ModContent.ProjectileType<Lighting_beam>(), (int)(Projectile.damage * 0.5f), 0f, 0.75f, 120f, 1);
		}
        public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 20; i++)
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(3f, 3f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.BlueTorch, speed, Scale: 1f);
				d.noGravity = true;
			}
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
		    Runa_lightbolt_proj._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Runa_lightbolt_proj._vertexStrip.DrawTrail();
		    Terraria.Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
            Color result = Color.LightCyan;
		    return result;
	    }
	    private float StripWidth(float progressOnStrip)
	    {
		    float num = 1f;
		    float lerpValue = Terraria.Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
		    num *= 1f - (1f - lerpValue) * (1f - lerpValue);
		    return MathHelper.Lerp(0f, 24f, num);
	    }
	}
}
