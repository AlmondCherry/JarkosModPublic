using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Mage_slayer_proj : ModProjectile {
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 100;
			Projectile.light = 0.95f;
			DrawOffsetX = -(int)((16 / 2) - (Projectile.Size.X / 2));
			DrawOriginOffsetY = -(int)((62 / 2) - (Projectile.Size.Y / 2));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Projectile.owner == Main.myPlayer)
			{
				float hitType = Main.rand.Next(1, 3);
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<Mage_slayer_hit_visual>(), 0, 0f, Projectile.owner, hitType);
			}
		}
		 public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float size = Projectile.scale * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, size, SpriteEffects.None, 0);
			}
			Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
			int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
			Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
			Vector2 origin2 = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
			Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}

	public class Mage_slayer_hit_visual : ModProjectile
	{
		public override string Texture => "Terraria/Images/Item_0";
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}
		public override void AI()
		{
			if (Projectile.timeLeft == 2 && Main.netMode != NetmodeID.Server)
			{
				float hitType = Projectile.ai[0];
				float modifier = Main.rand.NextFloat(0.5f, 1.25f);
				float size = Main.rand.NextFloat(0.25f, 0.75f);
				for (int i = 0; i < 2; i++)
				{
					new Spark_gravit(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f))) * modifier, Color.MediumPurple, 300, size * 2f, 2f, 0f).Spawn();
				}
				for (int i = 0; i < 2; i++)
				{
					new Spark_gravit(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f))) * modifier, Color.HotPink, 300, size * 4f, 2f, 0f).Spawn();
				}
				if (hitType == 1)
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/mage_slayer_hit_one") with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
				}
				else
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/mage_slayer_hit_two") with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
				}
			}
		}
	}
}
