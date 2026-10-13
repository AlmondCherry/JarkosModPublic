using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Rocket_arrow : ModProjectile
	{
		private const float ExplosionRadius = 100f;

		private float FlightTimer
		{
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		private float InitialSpeed
		{
			get => Projectile.localAI[1];
			set => Projectile.localAI[1] = value;
		}

		private bool exploded = false;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
			Projectile.arrow = true;
			Projectile.light = 0.1f;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			FlightTimer++;
			if (InitialSpeed == 0f && Projectile.velocity.Length() > 0f)
			{
				InitialSpeed = Projectile.velocity.Length();
			}


			const float AccelStartTick = 50f;

			if (FlightTimer <= AccelStartTick)
			{

				Projectile.velocity.Y += 0.008f;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Lighting.AddLight(Projectile.Center, 0.15f, 0.08f, 0.02f);
			}
			else
			{

				float accelProgress = MathHelper.Clamp((FlightTimer - AccelStartTick) / 35f, 0f, 1f);
				float smoothAccel = accelProgress * accelProgress;
				float baseSpeed = InitialSpeed > 0f ? InitialSpeed : 10f;
				float targetSpeed = MathHelper.Lerp(baseSpeed, baseSpeed * 2.8f, smoothAccel);

				if (Projectile.velocity != Vector2.Zero)
				{
					Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * targetSpeed, 0.25f);
				}

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;


				Lighting.AddLight(Projectile.Center, 1.4f * accelProgress, 0.85f * accelProgress, 0.2f * accelProgress);


				if (Main.rand.NextBool(8))
				{
					for (int i = 0; i < 2; i++)
					{
						Color particleColor = Main.rand.Next(3) switch
						{
							0 => new Color(255, 140, 20),
							1 => new Color(255, 210, 30),
							_ => new Color(255, 45, 30)
						};

						Vector2 spawnPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0f, 12f);
						Vector2 vel = -Projectile.velocity * Main.rand.NextFloat(0.08f, 0.25f) + Main.rand.NextVector2Circular(1.2f, 1.2f);

						new Electric_particle(spawnPos, vel, particleColor, Main.rand.Next(12, 22), Main.rand.NextFloat(0.35f, 0.6f), 1f, 0.90f).Spawn();
					}
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			CreateExplosion();
		}

		public override void OnKill(int timeLeft)
		{
			if (!exploded)
			{
				CreateExplosion();
			}
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}

		private void CreateExplosion()
		{
			if (exploded) return;
			exploded = true;


			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.85f, Pitch = -0.15f }, Projectile.Center);


			DealExplosionDamage();



			for (int i = 0; i < 5; i++)
			{
				Color color = Main.rand.Next(3) switch
				{
					0 => new Color(255, 140, 20),
					1 => new Color(255, 215, 35),
					_ => new Color(255, 45, 30)
				};
				Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
				new Electric_particle(Projectile.Center, vel, color, Main.rand.Next(20, 36), Main.rand.NextFloat(0.45f, 0.8f), 1f, 0.91f).Spawn();
			}


			for (int i = 0; i < 2; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(7.5f, 7.5f);
				Color sparkCol = Color.Lerp(new Color(255, 160, 30), new Color(255, 220, 80), Main.rand.NextFloat());
				new Spark(Projectile.Center, vel, sparkCol, Main.rand.Next(15, 28), Main.rand.NextFloat(0.4f, 0.75f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
			}


			new Circleedge_reducing(Projectile.Center, Vector2.Zero, new Color(255, 160, 40), 18, 1.2f, 1f, 0f).Spawn();


			for (int i = 0; i < 3; i++)
			{
				Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.4f);
				fire.noGravity = true;
			}
		}

		private void DealExplosionDamage()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage)
				{
					float distance = Vector2.Distance(Projectile.Center, npc.Center);
					if (distance < ExplosionRadius)
					{
						float damageMultiplier = 1f - (distance / ExplosionRadius);
						int explosionDamage = (int)(Projectile.damage * 0.75f * damageMultiplier);
						if (explosionDamage > 0)
						{
							int direction = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
							npc.SimpleStrikeNPC(explosionDamage, direction, false, Projectile.knockBack * 0.5f, Projectile.DamageType);
						}
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;


			Color trailColor = new Color(255, 140, 30);
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				float alpha = progress * 0.55f;
				Color color = trailColor * alpha;
				float scale = Projectile.scale * (0.5f + progress * 0.5f);
				Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Main.EntitySpriteDraw(texture, oldDrawPos, null, color, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
			}


			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				SpriteBatch sb = Main.spriteBatch;
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Vector2 glowOrigin = glowTex.Size() / 2f;
				Vector2 backOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 6f;
				Vector2 glowPos = Projectile.Center + backOffset - Main.screenPosition;


				Color softOrangeGlow = new Color(255, 120, 20) * 0.75f;
				Color innerYellowCore = new Color(255, 200, 60) * 0.45f;
				float glowScale = 0.55f * Projectile.scale;

				sb.Draw(glowTex, glowPos, null, softOrangeGlow, Projectile.rotation, glowOrigin, glowScale, SpriteEffects.None, 0f);
				sb.Draw(glowTex, glowPos, null, innerYellowCore, Projectile.rotation, glowOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}


			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
