using System;
using System.Collections.Generic;
using System.IO;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	#region Main Dagger Projectile
	public class JokerGambit_proj : ModProjectile
	{
		private const int TrailMax = 20;
		private List<Vector2> trailPositions = new List<Vector2>();
		private HashSet<int> markedNPCs = new HashSet<int>();
		private bool hasShattered = false;
		private float hitStopTimer = 0f;
		private Vector2 storedFlightVel = Vector2.Zero;
		private ref float Timer => ref Projectile.ai[1];

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = TrailMax;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
		}

		public override void AI()
		{
			
			if (hitStopTimer > 0f)
			{
				hitStopTimer--;
				Projectile.velocity *= 0.82f; 

				
				if (Main.rand.NextBool(2))
				{
					Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.GemRuby, Main.rand.NextVector2Circular(3, 3), 0, default, 1.3f);
					d.noGravity = true;
				}

				if (hitStopTimer <= 0f)
				{
					
					Vector2 resumeDir = storedFlightVel != Vector2.Zero ? Vector2.Normalize(storedFlightVel) : (Projectile.velocity != Vector2.Zero ? Vector2.Normalize(Projectile.velocity) : Vector2.UnitX.RotatedBy(Projectile.rotation - MathHelper.PiOver4));
					Projectile.velocity = resumeDir * 20f;
				}

				
				trailPositions.Insert(0, Projectile.Center);
				if (trailPositions.Count > TrailMax)
				{
					trailPositions.RemoveAt(trailPositions.Count - 1);
				}
				Lighting.AddLight(Projectile.Center, 1.0f, 0.25f, 0.3f);
				return;
			}

			Timer++;

			
			trailPositions.Insert(0, Projectile.Center);
			if (trailPositions.Count > TrailMax)
			{
				trailPositions.RemoveAt(trailPositions.Count - 1);
			}

			
			if (Projectile.velocity != Vector2.Zero)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
			}

			
			Lighting.AddLight(Projectile.Center, 0.9f, 0.2f, 0.25f);
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), DustID.GemRuby, -Projectile.velocity * 0.15f, 100, default, Main.rand.NextFloat(0.7f, 1.2f));
				d.noGravity = true;
			}

			
			Projectile.velocity *= 0.988f;
			if (Timer > 38 || Projectile.velocity.Length() < 3f)
			{
				ShatterIntoShards();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!markedNPCs.Contains(target.whoAmI))
			{
				markedNPCs.Add(target.whoAmI);
				SoundEngine.PlaySound(SoundID.Item101 with { Volume = 0.6f, Pitch = 0.3f }, target.Center);
				
				
				for (int i = 0; i < 12; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
					Dust d = Dust.NewDustPerfect(target.Center, DustID.GemRuby, vel, 0, default, 1.3f);
					d.noGravity = true;
				}
			}

			
			if (hitStopTimer <= 0f)
			{
				hitStopTimer = 24f; 
				if (Projectile.velocity != Vector2.Zero)
				{
					storedFlightVel = Projectile.velocity;
				}
				Projectile.velocity *= 0.12f; 
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			ShatterIntoShards();
			return false;
		}

		private void ShatterIntoShards()
		{
			if (hasShattered) return;
			hasShattered = true;

			
			SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);
			SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.1f, Volume = 0.7f }, Projectile.Center);

			for (int i = 0; i < 18; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, vel, 0, default, Main.rand.NextFloat(1.2f, 2.2f));
				d.noGravity = true;
			}
			for (int i = 0; i < 10; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
				new Spark(Projectile.Center, vel, new Color(255, 60, 40), 20, Main.rand.NextFloat(0.6f, 1.2f), 1f).Spawn();
			}

			
			if (Main.myPlayer == Projectile.owner)
			{
				for (int i = 0; i < 4; i++)
				{
					float angle = MathHelper.TwoPi / 4f * i + Main.rand.NextFloat(-0.4f, 0.4f);
					Vector2 shardVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3.5f, 6.5f);
					shardVel.Y -= Main.rand.NextFloat(1.5f, 3.5f); 

					int pIndex = Projectile.NewProjectile(
						Projectile.GetSource_FromThis(), 
						Projectile.Center, 
						shardVel, 
						ModContent.ProjectileType<JokerGambitShard>(), 
						Projectile.damage, 
						Projectile.knockBack, 
						Projectile.owner, 
						ai0: i 
					);

					if (pIndex >= 0 && pIndex < Main.maxProjectiles && Main.projectile[pIndex].ModProjectile is JokerGambitShard shard)
					{
						shard.SetMarkedNPCs(markedNPCs);
						shard.BaseItemDamage = Projectile.damage;
					}
				}
			}

			Projectile.Kill();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;

			
			if (trailPositions.Count > 1)
			{
				trailPositions[0] = Projectile.Center;
				if (HealShaders.Stream != null && HealShaders.HealNoise != null)
				{
					Effect effect = HealShaders.Stream;
					sb.End();
					effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
					effect.Parameters["uOpacity"]?.SetValue(1.6f);
					effect.Parameters["uColor"]?.SetValue(new Color(255, 40, 30).ToVector4());
					effect.Parameters["uColorCore"]?.SetValue(new Color(255, 220, 200).ToVector4());

					TrailRenderer.Draw(
						effect, 
						trailPositions, 
						t => MathHelper.Lerp(14f, 0f, t), 
						t => new Color(255, 40, 30) * (1f - t), 
						HealShaders.HealNoise
					);

					sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
					sb.End();
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}

			
			Texture2D texture = ModContent.Request<Texture2D>("jarkos/items/weapons/melee/true_melee/JokerGambit").Value;
			Vector2 origin = texture.Size() * 0.5f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			
			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			sb.Draw(texture, drawPos, null, new Color(255, 50, 40) * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0f);
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			
			sb.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}
	}
	#endregion

	#region Shard Projectile
	public class JokerGambitShard : ModProjectile
	{
		public override string Texture => "jarkos/projectiles/melee/true_melee/JokerG01";

		private HashSet<int> markedNPCs = new HashSet<int>();
		public int BaseItemDamage = 52;
		private float spinSpeed = 0f;
		private bool ignitionSoundPlayed = false;
		private ref float ShardIndex => ref Projectile.ai[0];
		private ref float Timer => ref Projectile.ai[1];

		public void SetMarkedNPCs(HashSet<int> npcs)
		{
			markedNPCs = new HashSet<int>(npcs);
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = false; 
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			Timer++;

			if (spinSpeed == 0f)
			{
				spinSpeed = Main.rand.NextFloat(-0.25f, 0.25f);
				if (Math.Abs(spinSpeed) < 0.08f) spinSpeed = 0.15f;
			}

			
			if (Timer < 42f)
			{
				Projectile.velocity.Y += 0.26f; 
				Projectile.velocity.X *= 0.98f;
				Projectile.rotation += spinSpeed;

				
				if (Main.rand.NextBool(3))
				{
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, Main.rand.NextVector2Circular(2f, 2f), 100, default, 0.7f);
					d.noGravity = true;
				}
			}
			
			else
			{
				if (!ignitionSoundPlayed)
				{
					ignitionSoundPlayed = true;
					SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 0.4f, Volume = 0.6f }, Projectile.Center);

					
					for (int i = 0; i < 8; i++)
					{
						Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
						new Spark(Projectile.Center, sparkVel, new Color(255, 140, 20), 16, Main.rand.NextFloat(0.5f, 0.9f), 1f).Spawn();
					}
				}

				Vector2 toPlayer = owner.Center - Projectile.Center;
				float distance = toPlayer.Length();

				
				if (distance < 35f)
				{
					ExecuteReturnEffects(owner);
					Projectile.Kill();
					return;
				}

				
				float targetAngle = toPlayer.ToRotation() + MathHelper.PiOver4;
				float diff = MathHelper.WrapAngle(targetAngle - Projectile.rotation);
				Projectile.rotation += diff * 0.12f;

				
				if (Timer < 60f)
				{
					float turnProgress = MathHelper.Clamp((Timer - 42f) / 18f, 0f, 1f);
					float turnSpeed = MathHelper.Lerp(1.5f, 8f, turnProgress);
					Vector2 targetVel = toPlayer.SafeNormalize(Vector2.UnitX) * turnSpeed;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, 0.08f + turnProgress * 0.08f);
				}
				
				else
				{
					float accelProgress = MathHelper.Clamp((Timer - 60f) / 35f, 0f, 1f);
					float returnSpeed = MathHelper.Lerp(8f, 32f, accelProgress);
					Vector2 targetVel = toPlayer.SafeNormalize(Vector2.UnitX) * returnSpeed;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, 0.14f);
				}

				
				if (Main.rand.NextBool(2))
				{
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.2f, 0, Color.OrangeRed, Main.rand.NextFloat(0.8f, 1.4f));
					d.noGravity = true;
				}
			}

			Lighting.AddLight(Projectile.Center, 0.8f, Timer >= 42f ? 0.4f : 0.1f, 0.1f);
		}

		private void ExecuteReturnEffects(Player owner)
		{
			
			SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f, Volume = 0.7f }, owner.Center);
			SoundEngine.PlaySound(SoundID.Item101 with { Pitch = 0.5f, Volume = 0.5f }, owner.Center);

			
			for (int i = 0; i < 10; i++)
			{
				Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
				new Spark(owner.Center, sparkVel, new Color(255, 130, 20), 18, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
			}

			
			int returnDamage = (int)(BaseItemDamage * 0.25f);
			if (returnDamage < 1) returnDamage = 1;

			if (markedNPCs.Count > 0)
			{
				foreach (int npcIndex in markedNPCs)
				{
					if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
					{
						NPC target = Main.npc[npcIndex];
						if (target.active && !target.dontTakeDamage && !target.friendly)
						{
							int hitDir = target.Center.X > owner.Center.X ? 1 : -1;
							
							
							target.SimpleStrikeNPC(returnDamage, hitDir, false, 2f, DamageClass.Melee);

							
							SoundEngine.PlaySound(SoundID.Item101 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);
							for (int i = 0; i < 8; i++)
							{
								Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
								Dust d = Dust.NewDustPerfect(target.Center, DustID.GemRuby, vel, 0, default, 1.4f);
								d.noGravity = true;
							}
							new SlashSparkParticle(target.Center, Main.rand.NextVector2CircularEdge(8f, 8f), new Color(255, 120, 30), 6, 0.7f, 4.5f).Spawn();
						}
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;

			
			if (Timer >= 42f)
			{
				Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
				Vector2 glowOrigin = glowTex.Size() * 0.5f;
				Vector2 glowPos = Projectile.Center - Main.screenPosition;

				float glowProgress = MathHelper.Clamp((Timer - 42f) / 15f, 0f, 1f);
				float pulse = (float)Math.Sin(Timer * 0.2f) * 0.08f + 0.45f;
				float scale = pulse * glowProgress;

				sb.End();
				sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				sb.Draw(glowTex, glowPos, null, new Color(255, 120, 20) * glowProgress, 0f, glowOrigin, scale, SpriteEffects.None, 0f);
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			
			int idx = Math.Clamp((int)ShardIndex, 0, 3) + 1;
			Texture2D shardTex = ModContent.Request<Texture2D>($"jarkos/projectiles/melee/true_melee/JokerG0{idx}").Value;
			Vector2 origin = shardTex.Size() * 0.5f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			
			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Color glowColor = Timer >= 42f ? new Color(255, 140, 30) : new Color(220, 30, 40);
			sb.Draw(shardTex, drawPos, null, glowColor * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			
			sb.Draw(shardTex, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}
	}
	#endregion
}
