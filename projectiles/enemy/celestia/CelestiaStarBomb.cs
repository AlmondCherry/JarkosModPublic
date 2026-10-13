using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.celestia
{
	public class CelestiaStarBomb : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
		public override void SetDefaults()
		{
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 480;
		}
		public override void OnSpawn(IEntitySource source)
		{
			Projectile.localAI[1] = (float)Main.rand.NextDouble() * MathHelper.TwoPi;
		}
		public override void AI()
		{
			Projectile.localAI[0]++;
			float gatherTime = Projectile.ai[1];
			float age = Projectile.localAI[0];
			if (gatherTime > 0f && age <= gatherTime)
			{
				Projectile.velocity *= 0.9f;
				Projectile.rotation += 0.012f;
				float t = age / gatherTime;
				Lighting.AddLight(Projectile.Center, 0.18f * t, 0.12f * t, 0.45f * t);
				if (!Main.dedServ && Main.rand.NextBool(2))
				{
					Vector2 randomDir = Main.rand.NextVector2CircularEdge(110f * (1f - t * 0.5f), 110f * (1f - t * 0.5f));
					Vector2 toCenter = -randomDir.SafeNormalize(Vector2.Zero) * (1.2f + t * 1.8f);
					Dust dust = Dust.NewDustPerfect(Projectile.Center + randomDir, DustID.PurpleTorch, toCenter, (int)((1f - t) * 220f), default, 0.6f + t * 0.9f);
					dust.noGravity = true;
					dust.fadeIn = 0.4f * (1f - t);
				}
				if (age == (int)gatherTime - 6 && !Main.dedServ)
				{
					for (int i = 0; i < 18; i++)
					{
						Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
						Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0, default, 1.4f);
						burst.noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.Item25 with { Volume = 0.55f, Pitch = 0.4f }, Projectile.Center);
				}
				return;
			}
			float bombAge = gatherTime > 0f ? age - gatherTime : age;
			float delay = Projectile.ai[0] <= 0f ? 135f : Projectile.ai[0];
			Projectile.velocity *= 0.94f;
			Projectile.rotation += 0.045f;
			Projectile.scale = 0.85f + (float)Math.Sin(bombAge * 0.08f) * 0.08f;
			Lighting.AddLight(Projectile.Center, 0.55f, 0.45f, 1.05f);
			if (bombAge > delay - 45f)
			{
				Projectile.scale += (float)Math.Sin(bombAge * 0.55f) * 0.1f;
				if (!Main.dedServ)
				{
					for (int i = 0; i < 2; i++)
					{
						Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(28f, 28f), DustID.WhiteTorch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.8f);
						dust.noGravity = true;
					}
				}
			}
			if (bombAge >= delay)
				Explode();
		}
		private void Explode()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int count = 12;
				float speed = 7.0f;
				float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				for (int i = 0; i < count; i++)
				{
					Vector2 velocity = (rotation + MathHelper.TwoPi / count * i).ToRotationVector2() * speed;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CelestiaStarRay>(), Projectile.damage, 0f, Main.myPlayer);
				}
			}
			if (!Main.dedServ)
			{
				for (int i = 0; i < 46; i++)
				{
					Vector2 velocity = Main.rand.NextVector2Circular(7f, 7f);
					Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.WhiteTorch : DustID.PurpleTorch, velocity, 0, default, 1.55f);
					dust.noGravity = true;
				}
			}
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CelestiaStarBomb") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
			Projectile.Kill();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;
			float gatherTime = Projectile.ai[1];
			float age = Projectile.localAI[0];
			if (gatherTime > 0f && age <= gatherTime)
			{
				DrawGatherSilhouettes(texture, origin, age, gatherTime);
				return false;
			}
			float bombAge = gatherTime > 0f ? age - gatherTime : age;
			float delay = Projectile.ai[0] <= 0f ? 135f : Projectile.ai[0];
			float warning = MathHelper.Clamp((bombAge - (delay - 45f)) / 45f, 0f, 1f);
			float blink = warning > 0f ? (float)Math.Sin(Main.GameUpdateCount * 0.75f) * 0.5f + 0.5f : 0f;

			Color color = Color.Lerp(new Color(140, 80, 255, 0), Color.White, blink * warning);

			float auraPulse = (float)Math.Sin(Main.GameUpdateCount * 0.11f + Projectile.identity) * 0.5f + 0.5f;
			float bornFade = MathHelper.Clamp(bombAge / 18f, 0f, 1f);

			for (int ring = 0; ring < 2; ring++)
			{
				int copies = ring == 0 ? 5 : 8;
				float radius = 5f + ring * 7f + warning * (12f + ring * 6f) + auraPulse * 5f;
				float spin = Main.GameUpdateCount * (ring == 0 ? 0.05f : -0.035f);
				for (int i = 0; i < copies; i++)
				{
					Vector2 offset = (MathHelper.TwoPi / copies * i + spin).ToRotationVector2() * radius;
					Color ringColor = ring == 0 ? color : new Color(90, 80, 255, 0);
					Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, ringColor * (0.28f + warning * 0.45f) * bornFade, Projectile.rotation + spin, origin, Projectile.scale * (0.72f + warning * 0.38f) * bornFade, SpriteEffects.None, 0);
				}
			}
			for (int i = 0; i < 6; i++)
			{
				Vector2 glowOffset = (MathHelper.TwoPi / 6f * i + Main.GameUpdateCount * 0.04f).ToRotationVector2() * (3f + warning * 8f);
				Main.EntitySpriteDraw(texture, Projectile.Center + glowOffset - Main.screenPosition, null, Color.White * (0.2f + warning * 0.48f) * bornFade, Projectile.rotation, origin, Projectile.scale * (1.15f + warning * 0.55f) * bornFade, SpriteEffects.None, 0);
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * bornFade, Projectile.rotation, origin, Projectile.scale * (1f + warning * 0.18f) * bornFade, SpriteEffects.None, 0);
			return false;
		}
		private void DrawGatherSilhouettes(Texture2D texture, Vector2 origin, float age, float gatherTime)
		{
			float t = MathHelper.Clamp(age / gatherTime, 0f, 1f);
			float seed = Projectile.localAI[1];
			const int silhouettes = 7;

			for (int i = 0; i < silhouettes; i++)
			{
				float baseAngle = MathHelper.TwoPi / silhouettes * i + seed;
				float angle = baseAngle + t * 1.4f * (i % 2 == 0 ? 1f : -1f);
				float radius = MathHelper.Lerp(130f, 0f, MathHelper.SmoothStep(0f, 1f, t));
				Vector2 dir = angle.ToRotationVector2();
				Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);
				float waveAmp = 28f * (1f - t) * (1f - t);
				float waveFreq = 4.2f;
				float wave = (float)Math.Sin(t * MathHelper.TwoPi * waveFreq + i * 1.7f + seed) * waveAmp;
				Vector2 pos = Projectile.Center + dir * radius + perp * wave;
				float scale = MathHelper.Lerp(0f, 0.72f, MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(t * 1.25f, 0f, 1f)));
				float silhouetteAlpha = MathHelper.Clamp(t * 1.6f, 0f, 1f);
				Color silhouetteColor = Color.Lerp(new Color(60, 35, 110, 0), new Color(180, 130, 255, 0), t) * silhouetteAlpha * 0.85f;
				
				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, silhouetteColor, age * 0.05f + i, origin, scale, SpriteEffects.None, 0);
				Color trailColor = new Color(140, 90, 255, 0) * silhouetteAlpha * 0.4f;
				Vector2 trailPos = pos + dir * 6f;
				Main.EntitySpriteDraw(texture, trailPos - Main.screenPosition, null, trailColor, age * 0.05f + i, origin, scale * 0.6f, SpriteEffects.None, 0);
			}

			float coreFade = MathHelper.Clamp((t - 0.65f) / 0.35f, 0f, 1f);
			if (coreFade > 0f)
			{
				float blink = (float)Math.Sin(Main.GameUpdateCount * 0.45f) * 0.5f + 0.5f;
				Color core = Color.Lerp(new Color(140, 90, 255, 0), Color.White, blink) * coreFade;
				for (int ring = 0; ring < 2; ring++)
				{
					int copies = 6;
					float spin = Main.GameUpdateCount * 0.05f * (ring == 0 ? 1f : -1f);
					for (int i = 0; i < copies; i++)
					{
						Vector2 off = (MathHelper.TwoPi / copies * i + spin).ToRotationVector2() * (4f + ring * 3f);
						Main.EntitySpriteDraw(texture, Projectile.Center + off - Main.screenPosition, null, core * 0.55f, 0f, origin, (0.55f + coreFade * 0.4f), SpriteEffects.None, 0);
					}
				}
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * coreFade * blink, 0f, origin, 0.65f + coreFade * 0.45f, SpriteEffects.None, 0);
			}
		}
	}
}
