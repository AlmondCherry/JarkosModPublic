using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;

namespace jarkos.projectiles.melee.true_melee
{
	public class Eternal_reign_proj : ModProjectile
	{
		private float progress = 0f;

		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 55;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Projectile.ai[0]++;
			progress = MathHelper.Clamp(Projectile.ai[0] / 55f, 0f, 1f);


			if (Projectile.velocity != Vector2.Zero)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
			}


			if (Projectile.ai[0] > 10)
			{
				Projectile.velocity *= 0.94f;
			}


			float fade = MathHelper.Clamp(Projectile.timeLeft / 18f, 0f, 1f);
			Lighting.AddLight(Projectile.Center, new Color(40, 255, 60).ToVector3() * 0.75f * fade);


			if (Main.rand.NextBool(2))
			{
				Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(24f, 24f);
				Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch, Projectile.velocity * 0.15f, 100, default, Main.rand.NextFloat(1.1f, 1.8f) * fade);
				d.noGravity = true;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.3f }, target.Center);
			for (int i = 0; i < 6; i++)
			{
				Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, Main.rand.NextVector2Circular(5f, 5f), 100, default, 1.4f);
				d.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.35f, Pitch = 0.4f }, Projectile.Center);
			

			int count = Main.rand.Next(3, 7);
			for (int i = 0; i < count; i++)
			{
				Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2.5f, 6.5f);
				float curveRate = Main.rand.NextFloat(0.04f, 0.11f) * (Main.rand.NextBool() ? 1f : -1f);
				Color greenColor = new Color(50, 255, 80);
				float scale = Main.rand.NextFloat(0.18f, 0.35f);
				int lifetime = Main.rand.Next(28, 48);
				float spin = Main.rand.NextFloat(-0.08f, 0.08f);

				new CurvedStarParticle(Projectile.Center, vel, greenColor, lifetime, scale, 1f, curveRate, spin).Spawn();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;

			float prog = MathHelper.Clamp(Projectile.ai[0] / 55f, 0f, 1f);

			float fade = MathHelper.Clamp(Projectile.timeLeft / 18f, 0f, 1f) * MathHelper.Clamp(Projectile.ai[0] / 4f, 0f, 1f);

			if (fade <= 0.01f) return false;

			Vector2 forwardDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 perpDir = new Vector2(-forwardDir.Y, forwardDir.X);

			float currentSpan = MathHelper.Lerp(140f, 40f, MathHelper.Clamp(prog * 1.35f, 0f, 1f));

			float bendAmount = MathHelper.Lerp(10f, 42f, MathHelper.Clamp(prog * 1.5f, 0f, 1f));

			Vector2 centerPos = Projectile.Center - Main.screenPosition;
			int segments = 24;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


			for (int pass = 0; pass < 2; pass++)
			{
				Color passColor;
				float passThickMult;
				if (pass == 0)
				{
					passColor = new Color(40, 255, 60) * 0.8f;
					passThickMult = 1.0f;
				}
				else
				{
					passColor = new Color(180, 255, 190) * 0.95f;
					passThickMult = 0.5f;
				}

				passColor *= fade;

				for (int i = 0; i < segments; i++)
				{
					float u0 = (i / (float)segments) - 0.5f;
					float u1 = ((i + 1) / (float)segments) - 0.5f;


					float curve0 = (1f - 4f * u0 * u0) * bendAmount;
					float curve1 = (1f - 4f * u1 * u1) * bendAmount;

					Vector2 pt0 = centerPos + perpDir * (u0 * currentSpan) + forwardDir * curve0;
					Vector2 pt1 = centerPos + perpDir * (u1 * currentSpan) + forwardDir * curve1;

					Vector2 diff = pt1 - pt0;
					float len = diff.Length();
					if (len <= 0.01f) continue;

					float rot = diff.ToRotation();
					float edgeFactor0 = (1f - 4f * u0 * u0);
					float thick = MathHelper.Lerp(3f, 14f, edgeFactor0) * passThickMult * (1f - prog * 0.35f);

					Vector2 scale = new Vector2(len, thick);
					Vector2 origin = new Vector2(0f, 0.5f);

					sb.Draw(pixel, pt0, new Rectangle(0, 0, 1, 1), passColor, rot, origin, scale, SpriteEffects.None, 0f);
				}
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
