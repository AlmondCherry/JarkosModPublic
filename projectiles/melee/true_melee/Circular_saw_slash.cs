using System;
using System.Collections.Generic;
using System.IO;
using jarkos.items.weapons.melee.true_melee;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class Circular_saw_slash : ModProjectile
	{
		public float SWINGRANGE = (float)Math.PI * 1.05f;

		private enum AttackStage
		{
			Prepare, Impact, Unwind
		}

		private AttackStage CurrentStage
		{
			get => (AttackStage)Projectile.localAI[0];
			set
			{
				Projectile.localAI[0] = (float)value;
				Timer = 0;
			}
		}

		private ref float StartAngle => ref Projectile.ai[1];
		private ref float Timer => ref Projectile.ai[2];
		private ref float Progress => ref Projectile.localAI[1];
		private ref float Size => ref Projectile.localAI[2];
		
		private bool hitted = false;
		private bool hit_cap_condition = false;
		private float hit_delay_back;


		private float prepTime => 6f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float impactTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 8f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 72;
			Projectile.height = 154;
			Projectile.friendly = true;
			Projectile.timeLeft = 10000;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.netImportant = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			float targetAngle = Projectile.velocity.LengthSquared() > 0.001f ? Projectile.velocity.ToRotation() : 0f;
			if (Main.myPlayer == Projectile.owner)
			{
				targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
			}
			Projectile.spriteDirection = MathF.Cos(targetAngle) >= 0 ? 1 : -1;
			StartAngle = targetAngle - 0.45f * SWINGRANGE * Projectile.spriteDirection;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((sbyte)Projectile.spriteDirection);
			writer.Write(StartAngle);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.spriteDirection = reader.ReadSByte();
			StartAngle = reader.ReadSingle();
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, 1f, 0.75f, 0.5f);
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;

			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
			{
				Projectile.Kill();
				return;
			}

			switch (CurrentStage)
			{
				case AttackStage.Prepare:
					PrepareStrike();
					break;
				case AttackStage.Impact:
					ImpactStrike();
					break;
				default:
					UnwindStrike();
					break;
			}
			SetSwordPosition();

			if (hit_cap_condition)
			{
				hit_delay_back++;
				if (hit_delay_back > 4f)
				{
					hitted = false;
				}
			}
			if (!hitted)
			{
				Timer++;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;

			if (Projectile.spriteDirection > 0)
			{
				
				origin = new Vector2(8f, texture.Height - 6f);
				rotationOffset = MathHelper.ToRadians(45f);
				effects = SpriteEffects.None;
			}
			else
			{
				
				origin = new Vector2(texture.Width - 8f, texture.Height - 6f);
				rotationOffset = MathHelper.ToRadians(135f);
				effects = SpriteEffects.FlipHorizontally;
			}

			Main.spriteBatch.Draw(
				texture, 
				Projectile.Center - Main.screenPosition, 
				null, 
				lightColor * Projectile.Opacity, 
				Projectile.rotation + rotationOffset, 
				origin, 
				Projectile.scale * 0.55f, 
				effects, 
				0
			);
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (texture.Height * Projectile.scale * 0.55f);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 18f * Projectile.scale, ref collisionPoint);
		}

		public override void CutTiles()
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (texture.Height * Projectile.scale * 0.55f);
			Terraria.Utils.PlotTileLine(start, end, 18f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool? CanDamage()
		{
			if (CurrentStage == AttackStage.Prepare)
				return false;
			return base.CanDamage();
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
		}

		public void SetSwordPosition()
		{
			Projectile.rotation = StartAngle + Projectile.spriteDirection * Progress;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition;
			Projectile.scale = Size * 1.89f * Owner.GetAdjustedItemScale(Owner.HeldItem);
			Owner.heldProj = Projectile.whoAmI;
		}

		private void PrepareStrike()
		{
			float progress = MathHelper.Clamp(Timer / prepTime, 0f, 1f);
			
			float easeOut = MathF.Sin(progress * MathHelper.PiOver2);
			Progress = -0.32f * easeOut;
			Size = MathHelper.Lerp(0.85f, 1.05f, easeOut);

			if (Timer >= prepTime)
			{
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Simple_swing") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
				}
				CurrentStage = AttackStage.Impact;
			}
		}

		private void ImpactStrike()
		{
			float progress = MathHelper.Clamp(Timer / impactTime, 0f, 1f);
			
			float easeSwing = progress < 0.5f ? 4f * progress * progress * progress : 1f - MathF.Pow(-2f * progress + 2f, 3) / 2f;
			Progress = MathHelper.Lerp(-0.32f, SWINGRANGE, easeSwing);
			Size = 1.05f;
			Projectile.Opacity = 1f;

			if (Timer >= impactTime)
			{
				CurrentStage = AttackStage.Unwind;
			}
		}

		private void UnwindStrike()
		{
			float progress = MathHelper.Clamp(Timer / hideTime, 0f, 1f);
			float easeUnwind = 1f - MathF.Pow(1f - progress, 2);
			Progress = MathHelper.Lerp(SWINGRANGE, SWINGRANGE + 0.15f, easeUnwind);
			Size = MathHelper.Lerp(1.05f, 0.6f, easeUnwind);
			Projectile.Opacity = 1f - easeUnwind;

			if (Timer >= hideTime)
			{
				Projectile.Kill();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[Projectile.owner];
			owner.Heal(Main.rand.Next(1, 3));

			if (!hit_cap_condition)
			{
				hitted = true;
			}
			hit_cap_condition = true;

			if (Main.netMode != NetmodeID.Server)
			{
				owner.GetModPlayer<Screenshake_system>().stronger_shake++;
				for (int i = 0; i < 4; i++)
				{
					Vector2 new_vel = Main.MouseWorld - target.Center;
					new_vel.Normalize();
					float size_at_pitch = Main.rand.NextFloat(2.45f, 3.25f) / 2;
					float speed = Main.rand.NextFloat(12f, 18f) / 2;
					new Spark(target.Center, new_vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-75f, 75f))) * speed, Color.DarkRed, 300, size_at_pitch, 1f, 0f).Spawn();
				}
				for (int i = 0; i < 5; i++)
				{
					float size_at_pitch = Main.rand.NextFloat(0.05f, 0.15f) / 2;
					float speed = Main.rand.NextFloat(12f, 18f) / 2;
					new Cube_particle(target.Center, Main.rand.NextVector2Circular(16f, 16f), Color.DarkRed, 300, size_at_pitch, 5f, 0f).Spawn();
				}
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/circular_saw_slash_sound") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
			}
		}
	}
}
