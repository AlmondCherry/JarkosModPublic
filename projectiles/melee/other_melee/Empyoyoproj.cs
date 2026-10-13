using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using jarkos.particles;
using Terraria.DataStructures;
namespace jarkos.projectiles.melee.other_melee
{
	public class Empyoyoproj : ModProjectile
	{
		private static VertexStrip _vertexStrip = new VertexStrip();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 25f;
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 450f;
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 22f;
		}
		public override void SetDefaults() {
		    Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 99;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1.333f;
		}
		public int customTimer = 0;
		public override void AI()
        {
			Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0.2f);
            customTimer++;
            if (customTimer % 120 == 1 && Projectile.owner == Main.myPlayer)
            {
				for (int i = 0; i < 8; i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Emp_shoot>(), Projectile.damage, Projectile.knockBack * 0.5f, Projectile.owner, Projectile.identity, i);
				}
            }
			for (int j = 0; j < 3; j++)
			{
				Vector2 offset = Main.rand.NextVector2CircularEdge(90f, 90f);
				Vector2 spawnPos = Projectile.Center + offset;
				Vector2 velocity = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(8f, 14f);
				new Stretching_particle(spawnPos, velocity, Color.Orange, 12, Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(3f, 5f), 0.05f).Spawn();
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 10; i++)
			{
				Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY * -1) * Main.rand.NextFloat(6f, 12f);
				new Spark(target.Center, velocity, Main.rand.NextBool() ? Color.Yellow : Color.Gold, Main.rand.Next(15, 25), Main.rand.NextFloat(0.3f, 0.7f), 0.9f).Spawn();
			}
			if (Projectile.owner == Main.myPlayer)
			{
				Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 4f, 6f, 10, 800f, FullName));
			}
		}
		public override void PostAI() {
            if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame);
				dust.noGravity = true;
				dust.scale = 1.3f;
			}
		}
		public override bool PreDraw(ref Color lightcolor) {
			Main.spriteBatch.End();
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var origin = texture.Size() / 2f;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			DrawTrail(Projectile);
			return false;
		}
	    public void DrawTrail(Projectile proj)
	    {
		    MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
		    miscShaderData.UseSaturation(-2.8f);
		    miscShaderData.UseOpacity(4f);
		    miscShaderData.Apply();
		    _vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
		    _vertexStrip.DrawTrail();
		    Terraria.Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	    }
	    private Color StripColors(float progressOnStrip)
	    {
            return Color.Gold;
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
