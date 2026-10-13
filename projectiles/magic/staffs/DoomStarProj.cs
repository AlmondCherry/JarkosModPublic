using System;
using System.Collections.Generic;
using System.IO;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.staffs
{
	public class DoomStarProj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/magic/staffs/doomStar";

		private List<Vector2> trailPositions = new List<Vector2>();
		private List<int> hitNPCs = new List<int>();

		public Color StarColor = Color.Magenta;

		private bool hasInitialized = false;
		private float pulseTimer = 0f;
		private bool isSingleTargetMode = false;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 360;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(StarColor.R);
			writer.Write(StarColor.G);
			writer.Write(StarColor.B);
			writer.Write(Projectile.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			byte r = reader.ReadByte();
			byte g = reader.ReadByte();
			byte b = reader.ReadByte();
			StarColor = new Color(r, g, b);
			Projectile.scale = reader.ReadSingle();
		}

		public override void AI()
		{
			if (!hasInitialized)
			{
				hasInitialized = true;

				StarColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.65f);
				Projectile.scale = Main.rand.NextFloat(0.75f, 1.40f);
				Projectile.netUpdate = true;
			}

			pulseTimer += 0.18f;
			Projectile.rotation = 0f;

			int nearbyCount = CountNearbyTargets(850f);
			isSingleTargetMode = nearbyCount <= 1;

			NPC target = FindBestTarget(850f);
			if (target != null)
			{
				Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
				float currentSpeed = Math.Max(Projectile.velocity.Length(), 18f);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * currentSpeed, 0.25f);
			}

			trailPositions.Insert(0, Projectile.Center);
			if (trailPositions.Count > 20)
			{
				trailPositions.RemoveAt(trailPositions.Count - 1);
			}

			Lighting.AddLight(Projectile.Center, StarColor.ToVector3() * 0.8f);

			if (Main.rand.NextBool(18))
			{
				Vector2 sparkVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
				new Spark(Projectile.Center + Main.rand.NextVector2Circular(6, 6), sparkVel, StarColor, 12, Main.rand.NextFloat(0.4f, 0.8f) * Projectile.scale, 0.9f).Spawn();
			}
		}

		private int CountNearbyTargets(float maxDistance)
		{
			int count = 0;
			float maxDistSq = maxDistance * maxDistance;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && !npc.immortal)
				{
					if (Vector2.DistanceSquared(Projectile.Center, npc.Center) < maxDistSq)
					{
						count++;
					}
				}
			}
			return count;
		}

		private NPC FindBestTarget(float maxDistance)
		{
			NPC nearest = null;
			float closestDistSq = maxDistance * maxDistance;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && !npc.immortal)
				{
					
					if (hitNPCs.Contains(npc.whoAmI) && CountUnHitNearbyTargets(maxDistance) > 0)
						continue;

					float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
					if (distSq < closestDistSq)
					{
						closestDistSq = distSq;
						nearest = npc;
					}
				}
			}
			return nearest;
		}

		private int CountUnHitNearbyTargets(float maxDistance)
		{
			int count = 0;
			float maxDistSq = maxDistance * maxDistance;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && !npc.immortal && !hitNPCs.Contains(npc.whoAmI))
				{
					if (Vector2.DistanceSquared(Projectile.Center, npc.Center) < maxDistSq)
					{
						count++;
					}
				}
			}
			return count;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			float pulseScale = Projectile.scale * (1f + MathF.Sin(pulseTimer) * 0.18f);

			if (trailPositions.Count > 1)
			{
				trailPositions[0] = Projectile.Center;

				if (HealShaders.Stream != null && HealShaders.HealNoise != null)
				{
					Effect effect = HealShaders.Stream;
					spriteBatch.End();
					effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
					effect.Parameters["uOpacity"]?.SetValue(1.6f);
					effect.Parameters["uColor"]?.SetValue(StarColor.ToVector4());
					effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());

					TrailRenderer.Draw(
						effect, 
						trailPositions, 
						t => MathHelper.Lerp(16f * pulseScale, 0f, t), 
						t => StarColor * (1f - t), 
						HealShaders.HealNoise
					);

					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}

			
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() * 0.5f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			
			Main.EntitySpriteDraw(texture, drawPos, null, StarColor * 0.75f, 0f, origin, pulseScale * 1.3f, SpriteEffects.None, 0);
			
			Main.EntitySpriteDraw(texture, drawPos, null, Color.Lerp(Color.White, StarColor, 0.25f), 0f, origin, pulseScale, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!hitNPCs.Contains(target.whoAmI))
			{
				hitNPCs.Add(target.whoAmI);
			}

			SoundEngine.PlaySound(SoundID.Item10 with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 0.6f }, Projectile.Center);

			if (Main.player[Projectile.owner].active)
			{
				Main.player[Projectile.owner].GetModPlayer<Screenshake_system>().weak_shake = 1;
			}

			for (int i = 0; i < 1; i++)
			{
				Vector2 sparkVel = Main.rand.NextVector2CircularEdge(7f, 7f) * Main.rand.NextFloat(0.5f, 1.2f);
				new Spark(target.Center, sparkVel, StarColor, 20, Main.rand.NextFloat(0.6f, 1.2f) * Projectile.scale, 1f).Spawn();
			}

			for (int i = 0; i < 2; i++)
			{
				Vector2 novaVel = Main.rand.NextVector2Circular(6f, 6f);
				new Electric_particle(target.Center, novaVel, StarColor, 70, Main.rand.NextFloat(0.4f, 0.6f), 1.25f, 0.98f).Spawn();
			}

			if (isSingleTargetMode || CountUnHitNearbyTargets(850f) == 0)
			{
				Projectile.Kill();
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.4f, Volume = 0.4f }, Projectile.Center);
			for (int i = 0; i < 10; i++)
			{
				Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
				new Spark(Projectile.Center, sparkVel, StarColor, 16, Main.rand.NextFloat(0.5f, 1.0f) * Projectile.scale, 1f).Spawn();
			}
		}
	}
}
