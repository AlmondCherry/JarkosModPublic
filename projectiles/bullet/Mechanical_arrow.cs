using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using jarkos.particles;
using Jarkos.projectiles.bullet;

namespace jarkos.projectiles.bullet
{
	public class Mechanical_arrow : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			AIType = ProjectileID.Bullet;
			Projectile.width = 8;
			Projectile.height = 20;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 0.35f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity.Y += 0.010f;

			for (int i = 0; i < 2; i++)
			{
				Color particleColor = Main.rand.Next(3) switch
				{
					0 => new Color(255, 140, 20),
					1 => new Color(255, 210, 30),
					_ => new Color(255, 45, 30)
				};

				Vector2 spawnPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0f, 10f);
				Vector2 vel = -Projectile.velocity * Main.rand.NextFloat(0.08f, 0.25f) + Main.rand.NextVector2Circular(1f, 1f);

				new Electric_particle(spawnPos, vel, particleColor, Main.rand.Next(12, 22), Main.rand.NextFloat(0.35f, 0.6f), 1f, 0.90f).Spawn();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 8; i++) 
			{
				Color color = Main.rand.Next(3) switch
				{
					0 => new Color(255, 140, 20),
					1 => new Color(255, 210, 30),
					_ => new Color(255, 45, 30)
				};
				Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
				new Electric_particle(Projectile.Center, vel, color, Main.rand.Next(15, 30), Main.rand.NextFloat(0.4f, 0.7f), 1f, 0.91f).Spawn();
			}

			float rand = Main.rand.NextFloat(-18f, -2f);
			float rand2 = Main.rand.NextFloat(-64f, 64f);

			Vector2 sped_y = new Vector2(0, rand);
			Vector2 pos_x = new Vector2(rand2, 0);

			if (Main.rand.NextFloat() < 0.30f)
			{
				SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
				float rand3 = Main.rand.NextFloat(0.05f, 0.15f);
				float rand4 = Main.rand.NextFloat(0.45f, 1f);

				new High_smoke(Projectile.Center, sped_y, Color.White, 30, rand3, rand4, 0f).Spawn();

				Projectile.NewProjectile(null, target.position, Vector2.Zero, ModContent.ProjectileType<Mech_burst>(), Projectile.damage, 0f);

				for (int a = 0; a < 30; a++) 
				{
					Dust d = Dust.NewDustPerfect(Projectile.position + pos_x, DustID.Smoke, sped_y * 1.25f, Scale: 2.5f);
					d.noGravity = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() / 2f;

			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				SpriteBatch sb = Main.spriteBatch;
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Vector2 glowOrigin = glowTex.Size() / 2f;
				Color orangeGlow = new Color(255, 130, 20) * 0.45f;
				sb.Draw(glowTex, drawPos, null, orangeGlow, 0f, glowOrigin, 0.45f, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}

		public override void OnKill(int timeLeft)
		{
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
}
