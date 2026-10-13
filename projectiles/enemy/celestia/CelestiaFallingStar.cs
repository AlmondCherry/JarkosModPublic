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
	public class CelestiaFallingStar : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 360;
			Projectile.scale = 0.45f;
		}
		public override void AI()
		{
			Projectile.localAI[0]++;
			float age = Projectile.localAI[0];
			int mode = (int)Projectile.ai[0];
			bool isP2 = mode == 1;
			bool isP3 = mode == 2;
			if (isP3)
			{
				Projectile.tileCollide = false;
				int targetIndex = (int)Projectile.ai[1];
				Player target = null;
				if (targetIndex >= 0 && targetIndex < Main.maxPlayers)
				{
					Player candidate = Main.player[targetIndex];
					if (candidate.active && !candidate.dead)
						target = candidate;
				}
				if (target == null)
					target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
				if (target != null && target.active && !target.dead)
				{
					Vector2 toTarget = target.Center - Projectile.Center;
					Vector2 currentDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
					Vector2 targetDir = toTarget.SafeNormalize(Vector2.UnitY);
					float dot = Vector2.Dot(currentDir, targetDir);
					if (dot > 0.819f)
					{
						Vector2 desired = targetDir * 14.5f;
						Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.0055f);
					}
				}
				if (Projectile.velocity.Y < 13f)
					Projectile.velocity.Y += 0.07f;
				Projectile.scale = MathHelper.Clamp(0.55f + age * 0.0075f, 0.55f, 1.30f);
				Projectile.rotation += 0.22f;
				Lighting.AddLight(Projectile.Center, 0.62f * Projectile.scale, 0.32f * Projectile.scale, 1.10f * Projectile.scale);
				if (!Main.dedServ)
				{
					int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.WhiteTorch;
					Dust trail = Dust.NewDustPerfect(Projectile.Center, dustType, -Projectile.velocity * 0.20f, 0, default, 1.05f + Projectile.scale * 0.45f);
					trail.noGravity = true;
					if (Main.rand.NextBool(4))
					{
						Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.0f);
						Spark.noGravity = true;
					}
				}
				if (target != null && Vector2.DistanceSquared(Projectile.Center, target.Center) <= 70f * 70f)
					Explode();
				return;
			}
			if (Projectile.velocity.Y < 16f)
				Projectile.velocity.Y += 0.27f;
			Projectile.velocity.X *= 0.992f;
			if (isP2)
				Projectile.scale = MathHelper.Clamp(0.55f + age * 0.009f, 0.55f, 1.55f);
			else
				Projectile.scale = MathHelper.Clamp(0.45f + age * 0.0075f, 0.45f, 1.25f);
			Projectile.rotation += 0.18f;
			Lighting.AddLight(Projectile.Center, 0.55f * Projectile.scale, 0.42f * Projectile.scale, 1.05f * Projectile.scale);
			if (!Main.dedServ && Main.rand.NextBool(2))
			{
				int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.WhiteTorch;
				Dust trail = Dust.NewDustPerfect(Projectile.Center, dustType, -Projectile.velocity * 0.18f, 0, default, 1.05f + Projectile.scale * 0.4f);
				trail.noGravity = true;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Explode();
			return false;
		}
		private void Explode()
		{
			int mode = (int)Projectile.ai[0];
			bool isP2 = mode == 1;
			bool isP3 = mode == 2;
			if (isP3) isP2 = true;
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CelestiaStarDrop") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
			if (!Main.dedServ)
			{
				int dustCount = isP2 ? 32 : 22;
				for (int i = 0; i < dustCount; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(isP2 ? 6.5f : 5f, isP2 ? 6.5f : 5f);
					int dustType = Main.rand.NextBool() ? DustID.PinkTorch : DustID.WhiteTorch;
					Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 0, default, isP2 ? 1.7f : 1.45f);
					dust.noGravity = true;
				}
				int sparkCount = isP2 ? 12 : 8;
				for (int i = 0; i < sparkCount; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(3.5f, 1.5f) - new Vector2(0f, 1.5f);
					Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0, default, 1.2f);
					Spark.noGravity = false;
				}
			}
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int shards = isP2 ? Main.rand.Next(6, 10) : Main.rand.Next(5, 8);
				int shardDamage = Math.Max(1, Projectile.damage * 65 / 100);
				float shardSpeedMin = isP2 ? 7.0f : 5.2f;
				float shardSpeedMax = isP2 ? 9.6f : 7.4f;
				float angleSpread = isP2 ? 0.95f : 0.7f;
				for (int i = 0; i < shards; i++)
				{
					float angle = MathHelper.Lerp(-MathHelper.PiOver2 - angleSpread, -MathHelper.PiOver2 + angleSpread, i / (float)Math.Max(1, shards - 1));
					Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(shardSpeedMin, shardSpeedMax);
					vel.X += Main.rand.NextFloat(-0.8f, 0.8f);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0f, -6f), vel, ModContent.ProjectileType<CelestiaStarShard>(), shardDamage, 0f, Main.myPlayer, 0f, isP2 ? 1f : 0f);
				}
			}
			Projectile.Kill();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;
			float pulse = (float)Math.Sin(Projectile.localAI[0] * 0.25f) * 0.5f + 0.5f;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Color trailColor = Color.Lerp(new Color(120, 80, 255, 0), Color.White, progress) * progress * 0.55f;
				Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale * progress * 0.85f, SpriteEffects.None, 0);
			}
			for (int i = 0; i < 4; i++)
			{
				Vector2 offset = (MathHelper.PiOver2 * i + Projectile.localAI[0] * 0.05f).ToRotationVector2() * (3f + pulse * 4f);
				Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, new Color(255, 160, 230, 0) * 0.4f, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
	public class CelestiaStarShard : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 150;
			Projectile.scale = 0.55f;
		}
		public override void OnSpawn(IEntitySource source)
		{
			if (Projectile.ai[1] >= 0.5f)
			{
				Projectile.scale = 0.85f;
				Projectile.timeLeft = 240;
				Projectile.width = 16;
				Projectile.height = 16;
			}
		}
		public override void AI()
		{
			Projectile.localAI[0]++;
			bool isP2 = Projectile.ai[1] >= 0.5f;
			float gravity = isP2 ? 0.20f : 0.32f;
			float maxFall = isP2 ? 13f : 14f;
			if (Projectile.velocity.Y < maxFall)
				Projectile.velocity.Y += gravity;
			Projectile.velocity.X *= 0.985f;
			Projectile.rotation += 0.22f;
			float lightMult = isP2 ? 1.45f : 1f;
			Lighting.AddLight(Projectile.Center, 0.32f * lightMult, 0.22f * lightMult, 0.6f * lightMult);
			if (Projectile.timeLeft < 30)
				Projectile.alpha = (int)MathHelper.Clamp((30 - Projectile.timeLeft) * 8.5f, 0f, 255f);
			if (!Main.dedServ && Main.rand.NextBool(3))
			{
				int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.WhiteTorch;
				Dust trail = Dust.NewDustPerfect(Projectile.Center, dustType, -Projectile.velocity * 0.12f, Projectile.alpha, default, 0.95f);
				trail.noGravity = true;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.ai[0]++;
			if (Projectile.ai[0] >= 2f)
			{
				Projectile.Kill();
				return false;
			}
			if (Math.Abs(oldVelocity.X - Projectile.velocity.X) > 0.1f)
				Projectile.velocity.X = -oldVelocity.X * 0.45f;
			if (Math.Abs(oldVelocity.Y - Projectile.velocity.Y) > 0.1f)
				Projectile.velocity.Y = -oldVelocity.Y * 0.55f;
			return false;
		}
		public override void OnKill(int timeLeft)
		{
			if (Main.dedServ)
				return;
			for (int i = 0; i < 8; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
				int dustType = Main.rand.NextBool() ? DustID.PinkTorch : DustID.WhiteTorch;
				Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 0, default, 1.15f);
				dust.noGravity = true;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;
			float fade = 1f - Projectile.alpha / 255f;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Color trailColor = Color.Lerp(new Color(140, 100, 255, 0), Color.White, progress) * progress * 0.5f * fade;
				Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale * progress * 0.8f, SpriteEffects.None, 0);
			}
			for (int i = 0; i < 3; i++)
			{
				Vector2 offset = (MathHelper.TwoPi / 3f * i + Projectile.localAI[0] * 0.08f).ToRotationVector2() * 2.5f;
				Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, new Color(255, 160, 220, 0) * 0.4f * fade, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
