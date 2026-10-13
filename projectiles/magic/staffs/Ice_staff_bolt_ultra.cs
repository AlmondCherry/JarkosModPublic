using jarkos.particles.icestaff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.magic.staffs
{
	public class Ice_staff_bolt_ultra : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
            Projectile.scale = 1f;
			Projectile.damage = 24;
            Projectile.penetrate = 1;
			Projectile.light = 1f;
			Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 350;
			Projectile.tileCollide = false;
		}
		public override void AI(){
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(1)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.BlueTorch, Scale: 0.7f);
            dust.velocity /= 20;
			dust.noGravity = true;}
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
			for (int i = 0; i < 3; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(12f, 12f);
				float rand_rot = Main.rand.NextFloat(2f, 68f);
                float rand_siz = Main.rand.NextFloat(0.1f, 2f);
                new Snow_particle(target.Center, vel, Color.Cyan, 160, rand_siz, 4f, rand_rot).Spawn();
            }
			for (int i = 0; i < 3; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(12f, 12f);
				float rand_rot = Main.rand.NextFloat(2f, 68f);
                float rand_siz = Main.rand.NextFloat(0.1f, 2f);
                new Snow_particle(target.Center, vel, Color.LightCyan, 160, rand_siz, 4f, rand_rot).Spawn();
            }
		}
		private static VertexStrip _vertexStrip = new VertexStrip();
        public override bool PreDraw(ref Color lightcolor) {
            Draw(Projectile);
            return true;
        }
	    public void Draw(Projectile proj)
	    {
		    MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();
		    Ice_staff_bolt_ultra._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Ice_staff_bolt_ultra._vertexStrip.DrawTrail();
		    Terraria.Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
            Color result = Color.Cyan;
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
