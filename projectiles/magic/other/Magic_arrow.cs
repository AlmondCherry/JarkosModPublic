using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using jarkos.systems;
using jarkos.particles;
namespace jarkos.projectiles.magic.other
{
	public class Magic_arrow : ModProjectile
	{
        private float rand = Main.rand.NextFloat(-180, 180);
        private float rand1 = Main.rand.NextFloat(-180, 180);
        private float rand2 = Main.rand.NextFloat(-180, 180);
		public override void SetStaticDefaults() 
        {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
            Projectile.width = 8;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 5;
            Projectile.tileCollide = false;
            Projectile.ArmorPenetration = 10;
            Projectile.penetrate = 1;
            Projectile.light = 0.75f;
            Projectile.timeLeft = 250;
        }
        public override void AI()
        {   
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 1.03f;

		    NPChoming.HomingProj(Projectile, 2000f, 3f, 240);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for(int i = 0; i < 1; i++)
            {
                float randSize = Main.rand.NextFloat(0.02f, 0.15f);

                Vector2 y = Main.rand.NextVector2Unit(rand, rand);
                Vector2 y1 = Main.rand.NextVector2Unit(rand1, rand1);
                Vector2 y2 = Main.rand.NextVector2Unit(rand2, rand2);

                new StarParticle(Projectile.Center, y * 10, Color.Cyan, 30, randSize, 1f, 0f).Spawn();
                new StarParticleVertical(Projectile.Center, y1 * 10, Color.White, 30, randSize, 1f, 0f).Spawn();
                new StarParticleHorizontal(Projectile.Center, y2 * 10, Color.LightCyan, 30, randSize, 1f, 0f).Spawn();

            }

            for (int i = 0; i < 8; i++) 
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);

                Dust d = Dust.NewDustPerfect(Projectile.position, DustID.BlueTorch, speed * 5f, Scale: 1f);
                d.noGravity = true;
        	}
		}
		public override bool PreDraw(ref Color lightcolor) 
        {
			Main.spriteBatch.End();

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var origin = texture.Size() / 2f;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Draw(Projectile);

			return false;
        }
        private static VertexStrip _vertexStrip = new VertexStrip();
	    public void Draw(Projectile proj)
	    {
		    MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();

		    Magic_arrow._vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    Magic_arrow._vertexStrip.DrawTrail();

		    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
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

		    return MathHelper.Lerp(0f, 32f, num);
	    }
    }
}
