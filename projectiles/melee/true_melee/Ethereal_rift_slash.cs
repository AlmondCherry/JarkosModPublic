using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Ethereal_rift_slash : ModProjectile {
		public float SWINGRANGE = (float)Math.PI;
		private const float FIRSTHALFSWING = 0.45f;
		private const float WINDUP = 0.001f;
		private const float UNWIND = 0.001f;
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
		private int alpha_runtime = 255;
		private ref float StartAngle => ref Projectile.ai[1];
		private ref float Timer => ref Projectile.ai[2];
		private ref float Progress => ref Projectile.localAI[1];
		private ref float Size => ref Projectile.localAI[2];
		private const int DissapearTimer = 20;
		private float DissapearProgress;
		private bool hitted = false;
		private bool hit_cap_condition = false;
		private float hit_delay_back;
		private float slowing = 1f;
		private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float impactTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private Player Owner => Main.player[Projectile.owner];
        private const int FrameCount = 6;
        private const int FrameSpeed = 3;
		public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FrameCount;
        }
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = true;
			Projectile.timeLeft = 10000;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
		}
		public override void OnSpawn(IEntitySource source)
		{
			if (Projectile.owner == Main.myPlayer)
			{
				Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
				float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
				StartAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection;
				Projectile.netUpdate = true;
			}
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
			alpha_runtime -= 5;
			slowing = 1f;
			if (alpha_runtime < 0)
			{
				alpha_runtime = 0;
			}
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
				{
					Projectile.Kill();
					return;
				}
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FrameCount)
                    Projectile.frame = 0;
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
			if(hit_cap_condition)
			{
				hit_delay_back++;
				if(hit_delay_back > 4f)
				{
					hitted = false;
				}
			}
			if(!hitted)
			{
				Timer++;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = ModContent.Request<Texture2D>("jarkos/textures/Circle_slashglow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / FrameCount;
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            float currentAngle = Projectile.rotation;
            int dir = Owner.direction;
			Vector2 origin;
			float rotationOffset;
			float rotationOffsetGlow;
			SpriteEffects effects;
			if (Projectile.spriteDirection > 0)
			{
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(45f);
				rotationOffsetGlow = -MathHelper.ToRadians(90f);
				effects = SpriteEffects.None;
			}
			else
			{
				origin = new Vector2(Projectile.width, Projectile.height);
				rotationOffset = MathHelper.ToRadians(135f);
				rotationOffsetGlow = -MathHelper.ToRadians(90f);
				effects = SpriteEffects.FlipHorizontally;
			}
			float bladeLength = 1f * Projectile.scale * 1.2f;
			Vector2 tipPosition = Projectile.Center + Projectile.rotation.ToRotationVector2() * bladeLength;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Color glowColor = new Color(54, 155, 255);
			float opacity = (255 - Projectile.alpha) / 255f;
			Color drawColor = glowColor * opacity;
			float glowScale = Projectile.scale * 0.52f;
			Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
			Main.spriteBatch.Draw(glow, tipPosition - Main.screenPosition, null, drawColor * Projectile.Opacity, Projectile.rotation + rotationOffsetGlow, glowOrigin, glowScale, SpriteEffects.None, 0);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, sourceRect, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale * 1f, effects, 0);
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 1f);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}
		public override void CutTiles()
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Terraria.Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
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
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2);
			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition;
			Projectile.scale = Size * 1.89f * Owner.GetAdjustedItemScale(Owner.HeldItem);
			Owner.heldProj = Projectile.whoAmI;
		}
		private void PrepareStrike()
		{
			Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime);
			Size = MathHelper.SmoothStep(0.99f, 1, Timer / prepTime);
			if (Timer >= prepTime / 2)
			{
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.8f }, Projectile.Center);
				}
				CurrentStage = AttackStage.Impact;
			}
		}
		private void ImpactStrike()
		{
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / impactTime * slowing);
			DissapearProgress++;
			float ProgressDis = DissapearProgress / DissapearTimer;
			Projectile.alpha = (int)(255 * ProgressDis);
			if (Timer >= impactTime)
			{
				CurrentStage = AttackStage.Unwind;
			}
		}
		private void UnwindStrike()
		{
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
			Size = 1f - MathHelper.SmoothStep(0.85f, 1, Timer / hideTime);
			if (Timer >= hideTime)
			{
				Projectile.Kill();
			}
		}
		const float max_delay_attack = 1f;
		float delay_attack;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
			if(!hit_cap_condition)
			{
				hitted = true;
			}
			hit_cap_condition = true;
			target.AddBuff(BuffID.OnFire3, 300);
			if (Main.netMode != NetmodeID.Server)
			{
				if (Main.LocalPlayer.whoAmI == Projectile.owner)
				{
					owner.GetModPlayer<Screenshake_system>().stronger_shake++;
				}
				for (int i = 0; i < 5; i++)
				{
					Vector2 new_vel = Main.MouseWorld - target.Center;
					new_vel.Normalize();
					float size_at_pitch = Main.rand.NextFloat(1.45f, 2.25f) / 2;
					float speed = Main.rand.NextFloat(12f, 18f) / 2;
					new Spark_gravit(target.Center, new_vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-75f, 75f))) * speed, Color.Orange, 300, size_at_pitch, 1f, 0f).Spawn();
				}
				for (int i = 0; i < 10; i++)
				{
					Vector2 velocity = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-10f, -3f));
					Dust fire = Dust.NewDustDirect(target.Center - new Vector2(10), 20, 20, DustID.Torch, velocity.X, velocity.Y, 80, default, Main.rand.NextFloat(2f, 3f));
					fire.noGravity = true;
					fire.fadeIn = 1.8f;
				}
				for (int i = 0; i < 15; i++)
				{
					Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-12f, -4f));
					Dust solar = Dust.NewDustDirect(target.Center - new Vector2(8), 16, 16, DustID.SolarFlare, velocity.X, velocity.Y, 100, default, 2.2f);
					solar.noGravity = true;
				}
				SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.4f, Volume = 0.8f }, target.Center);
			}
        }
    }
}
