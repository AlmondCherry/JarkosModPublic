using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class Cirno_minion : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/Cirno_minion";

		public int level => Math.Clamp((int)Projectile.ai[0], 1, 4);

		private int oldLevel = 1;
		private int attackTimer = 0;
		private int attackPhase = 0;
		private float floatTimer = 0f;
		private float idleSpinTimer = 0f;
		private bool hasTarget = false;

		private float recoilTimer = 0f;
		private int tiltDirection = 1;

		public override void SetStaticDefaults()
		{
			Main.projPet[Projectile.type] = false;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 34;
			Projectile.height = 42;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 18000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (Projectile.active)
			{
				player.ownedProjectileCounts[Projectile.type]++;
			}

			if (player.dead || !player.active)
			{
				player.ClearBuff(ModContent.BuffType<Cirno_buff>());
			}

			if (player.HasBuff(ModContent.BuffType<Cirno_buff>()))
			{
				Projectile.timeLeft = 18000;
			}
			else
			{
				Projectile.Kill();
				return;
			}

			if (recoilTimer > 0f)
			{
				recoilTimer *= 0.86f;
				if (recoilTimer < 0.01f) recoilTimer = 0f;
			}

			int currentLevel = level;
			if (currentLevel != oldLevel)
			{
				oldLevel = currentLevel;

				CombatText.NewText(Projectile.getRect(), new Color(100, 220, 255), "Lv. " + currentLevel, true);

				SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.85f, Pitch = 0.4f }, Projectile.Center);
				new Circular_glow(Projectile.Center, Vector2.Zero, new Color(100, 220, 255), 22, 0.7f, 0.9f).Spawn();
				for (int i = 0; i < 14; i++)
				{
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.4f);
					d.noGravity = true;
				}
			}

			Projectile.minionSlots = currentLevel * 1f;

			if (currentLevel == 4)
			{
				player.AddBuff(ModContent.BuffType<Cirno_Level4_buff>(), 2);
			}

			float scaleMultiplier = 0.5f + (currentLevel - 1) * 0.075f;
			Projectile.scale = scaleMultiplier;

			float damageMultiplier = 1.0f + (currentLevel - 1) * 1.0f;
			if (Projectile.originalDamage > 0)
			{
				Projectile.damage = (int)(Projectile.originalDamage * damageMultiplier);
			}

			int bonusCrit = (currentLevel - 1) * 10;
			Projectile.CritChance = (int)(player.GetCritChance(DamageClass.Summon) + bonusCrit);

			floatTimer += 0.06f;

			NPC target = FindTarget(player);
			hasTarget = target != null;

			if (!hasTarget)
			{
				idleSpinTimer += 0.042f;
			}
			else
			{
				idleSpinTimer = 0f;
			}

			Vector2 hoverOffset = new Vector2(player.direction * -45f, -48f + (float)Math.Sin(floatTimer) * 8f);
			Vector2 targetPosition = player.Center + hoverOffset;

			if (target != null)
			{
				Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
			}
			else
			{
				Projectile.spriteDirection = player.direction;
			}

			Vector2 moveVel = targetPosition - Projectile.Center;
			float dist = moveVel.Length();
			if (dist > 1200f)
			{
				Projectile.Center = targetPosition;
			}
			else if (dist > 6f)
			{
				float moveSpeed = MathHelper.Clamp(dist * 0.15f, 4f, 20f);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveVel.SafeNormalize(Vector2.Zero) * moveSpeed, 0.18f);
			}
			else
			{
				Projectile.velocity *= 0.8f;
			}

			if (Main.rand.NextBool(4))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 0.8f);
				d.noGravity = true;
				d.velocity = Projectile.velocity * 0.2f;
			}

			if (target != null && Projectile.owner == Main.myPlayer)
			{
				float attackSpeedMultiplier = 1.0f + (currentLevel - 1) * 0.20f;
				int attackDelay = Math.Max(12, (int)(48f / attackSpeedMultiplier));

				attackTimer++;
				if (attackTimer >= attackDelay)
				{
					attackTimer = 0;
					ExecuteAttack(target, currentLevel);
				}
			}
		}

		private NPC FindTarget(Player player)
		{
			float maxDetectDist = 800f;
			NPC target = null;

			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < maxDetectDist)
				{
					return npc;
				}
			}

			float closestDist = maxDetectDist;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
				{
					float dist = Vector2.Distance(Projectile.Center, npc.Center);
					if (dist < closestDist)
					{
						closestDist = dist;
						target = npc;
					}
				}
			}
			return target;
		}

		private void ExecuteAttack(NPC target, int currentLevel)
		{
			Player player = Main.player[Projectile.owner];

			recoilTimer = 1.0f;
			tiltDirection = -tiltDirection;

			int baseDamage = Projectile.damage;
			int bonusCrit = (currentLevel - 1) * 10;
			int totalCrit = (int)(player.GetCritChance(DamageClass.Summon) + bonusCrit);

			Vector2 shootOrigin = Projectile.Center;
			Vector2 aimDir = (target.Center - shootOrigin).SafeNormalize(Vector2.UnitX);

			if (attackPhase == 0)
			{
				float[] speeds = new float[] { 7.5f, 10.5f, 13.5f };
				float spreadAngle = MathHelper.ToRadians(22f);

				for (int i = 0; i < 3; i++)
				{
					float angleOffset = MathHelper.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, i / 2f);
					Vector2 vel = aimDir.RotatedBy(angleOffset) * speeds[i];

					Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), shootOrigin, vel, ModContent.ProjectileType<Snowflake_proj>(), baseDamage, Projectile.knockBack, Projectile.owner);
					p.CritChance = totalCrit;
				}

				SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.6f, Pitch = 0.3f }, shootOrigin);
			}
			else if (attackPhase == 1)
			{
				Vector2 vel = aimDir * 3f;
				Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), shootOrigin, vel, ModContent.ProjectileType<BiggerSnowflake_proj>(), baseDamage, Projectile.knockBack * 1.2f, Projectile.owner);
				p.CritChance = totalCrit;

				SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.65f, Pitch = 0.2f }, shootOrigin);
			}
			else if (attackPhase == 2)
			{
				int shardDamage = (int)(baseDamage * 3.5f);
				float isBiggerFlag = (currentLevel >= 3) ? 1f : 0f;

				Vector2 vel = aimDir * 4f;
				Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), shootOrigin, vel, ModContent.ProjectileType<CirnoIceShard_proj>(), shardDamage, Projectile.knockBack * 1.5f, Projectile.owner, ai0: isBiggerFlag);
				p.CritChance = totalCrit;

				SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.75f, Pitch = -0.1f }, shootOrigin);
			}

			attackPhase = (attackPhase + 1) % 3;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			SpriteEffects fx = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			float spinScaleX = 1.0f;
			float squashYFactor = 1.0f;

			if (!hasTarget && recoilTimer <= 0.05f)
			{
				float cosVal = (float)Math.Cos(idleSpinTimer);
				float absCos = Math.Abs(cosVal);

				float easeOutCos = (float)Math.Sin(absCos * MathHelper.PiOver2);
				spinScaleX = MathHelper.Lerp(0.04f, 1.0f, easeOutCos);
				squashYFactor = MathHelper.Lerp(0.78f, 1.0f, spinScaleX);

				if (cosVal < 0f)
				{
					fx ^= SpriteEffects.FlipHorizontally;
				}
			}

			float tiltAngle = MathHelper.ToRadians(25f) * tiltDirection * recoilTimer;
			float recoilSquashY = MathHelper.Lerp(1.0f, 0.70f, recoilTimer);
			float recoilStretchX = MathHelper.Lerp(1.0f, 1.25f, recoilTimer);

			Vector2 drawScale = new Vector2(spinScaleX * recoilStretchX, squashYFactor * recoilSquashY) * Projectile.scale;

			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Color glowColor = new Color(60, 200, 255) * 0.4f;
				sb.Draw(glowTex, drawPos, null, glowColor, 0f, glowTex.Size() / 2f, 0.45f * Projectile.scale, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			float drawRotation = Projectile.rotation + tiltAngle;
			sb.Draw(tex, drawPos, null, lightColor, drawRotation, origin, drawScale, fx, 0f);

			return false;
		}
	}
}
