using System;
using System.Collections.Generic;
using System.IO;
using jarkos.items.weapons.melee.true_melee;
using jarkos.particles;
using jarkos.systems;
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
	public class Collapsar_slash : ModProjectile {
		public float SWINGRANGE = Math.Sign(Main.rand.NextFloat(-1f, 1f)) * (float)Math.PI;
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
		private float previousSlashProgress;
		private float slashGlowShaderIntensity;
		private float slowing = 1f;
		private float prepTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float impactTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 8f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private Player Owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.width = 92;
			Projectile.height = 104;
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
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
			StartAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection;
		}
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((sbyte)Projectile.spriteDirection);
			writer.Write(StartAngle);
			writer.Write(Timer);
			writer.Write(Projectile.rotation);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.spriteDirection = reader.ReadSByte();
			StartAngle = reader.ReadSingle();
			Timer = reader.ReadSingle();
			Projectile.rotation = reader.ReadSingle();
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
			UpdateSlashDistortion();
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
            float Direction = Owner.direction * Owner.gravDir;
            float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f;
			Vector2 dustVelocity = (dustRotation + Direction * MathHelper.PiOver2).ToRotationVector2();
			if (Main.rand.NextBool(2))
            {
				Color dustColor = Color.Lerp(Color.Gray, Color.White, Main.rand.NextFloat() * 0.3f);
				Dust coloredDust = Dust.NewDustPerfect(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * 80f * Projectile.scale + 20f * Projectile.scale), DustID.FireworksRGB, dustVelocity * 1f, 100, dustColor, 0.4f);
                new StarParticle(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * 80f * Projectile.scale + 20f * Projectile.scale), dustVelocity * 5f, Color.LightCyan, 250, 0.025f, 0.75f, 0f).Spawn();
				coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
				coloredDust.noGravity = true;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = ModContent.Request<Texture2D>("jarkos/textures/Circle_slashglow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
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
			Color glowColor = Color.Cyan;
			float glowOpacity = GetSlashGlowOpacity();
			Color drawColor = glowColor * glowOpacity;
			float glowScale = Projectile.scale * 0.48f;
			Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Effect glowShader = ModContent.Request<Effect>("Jarkos/Shaders/CollapsarSlashGlow_PS2Fallback", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Effect glowGlitchShader = ModContent.Request<Effect>("Jarkos/Shaders/CollapsarSlashGlowGlitch", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Effect glowWarpShader = ModContent.Request<Effect>("Jarkos/Shaders/CollapsarSlashGlowWarp", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Vector2 shaderDirection = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2();
			float glowIntensity = MathHelper.Clamp(slashGlowShaderIntensity * 1.25f, 0f, 1f);
			float glitchIntensity = MathHelper.Clamp(slashGlowShaderIntensity * 1.05f, 0f, 1f);
			float warpIntensity = MathHelper.Clamp(slashGlowShaderIntensity * 1.35f, 0f, 1f);
			glowShader.Parameters["uDirection"]?.SetValue(shaderDirection);
			glowShader.Parameters["uIntensity"]?.SetValue(glowIntensity);
			glowShader.Parameters["uOpacity"]?.SetValue(glowOpacity);
			glowShader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount / 60f);
			glowShader.Parameters["uColor"]?.SetValue(new Vector3(0.25f, 0.95f, 1f));
			glowShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.95f, 0.30f, 1f));
			glowShader.CurrentTechnique.Passes["Pass1"].Apply();
			Main.spriteBatch.Draw(glow, tipPosition - Main.screenPosition, null, drawColor, Projectile.rotation + rotationOffsetGlow, glowOrigin, glowScale, SpriteEffects.None, 0f);
			glowWarpShader.Parameters["uDirection"]?.SetValue(shaderDirection);
			glowWarpShader.Parameters["uIntensity"]?.SetValue(warpIntensity);
			glowWarpShader.Parameters["uOpacity"]?.SetValue(glowOpacity * 0.65f);
			glowWarpShader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount / 60f);
			glowWarpShader.Parameters["uColor"]?.SetValue(new Vector3(0.25f, 0.95f, 1f));
			glowWarpShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.95f, 0.30f, 1f));
			glowWarpShader.CurrentTechnique.Passes["Pass1"].Apply();
			Main.spriteBatch.Draw(glow, tipPosition - Main.screenPosition, null, drawColor * 0.55f, Projectile.rotation + rotationOffsetGlow, glowOrigin, glowScale, SpriteEffects.None, 0f);
			glowGlitchShader.Parameters["uDirection"]?.SetValue(shaderDirection);
			glowGlitchShader.Parameters["uIntensity"]?.SetValue(glitchIntensity);
			glowGlitchShader.Parameters["uOpacity"]?.SetValue(glowOpacity * 0.70f);
			glowGlitchShader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount / 60f);
			glowGlitchShader.Parameters["uColor"]?.SetValue(new Vector3(1f, 0.20f, 0.35f));
			glowGlitchShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.15f, 0.80f, 1f));
			glowGlitchShader.CurrentTechnique.Passes["Pass1"].Apply();
			Main.spriteBatch.Draw(glow, tipPosition - Main.screenPosition, null, drawColor * 0.45f, Projectile.rotation + rotationOffsetGlow, glowOrigin, glowScale, SpriteEffects.None, 0f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0f);
			return false;
		}
		private float GetSlashGlowOpacity()
		{
			if (CurrentStage != AttackStage.Impact)
				return 0f;
			float progress = MathHelper.Clamp(Timer / impactTime, 0f, 1f);
			float fadeIn = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(progress / 0.72f, 0f, 1f));
			float fadeOut = 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((progress - 0.78f) / 0.16f, 0f, 1f));
			return fadeIn * fadeOut;
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
                if (Main.rand.Next(100) < 50)
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Collapsar_sfx") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
                }
                else
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Collapsar_sfxtwo") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
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
		private void UpdateSlashDistortion()
		{
			if (CurrentStage != AttackStage.Impact)
			{
				previousSlashProgress = Progress;
				slashGlowShaderIntensity *= 0.75f;
				return;
			}
			float progressDelta = MathF.Abs(Progress - previousSlashProgress);
			previousSlashProgress = Progress;
			if (progressDelta <= 0.002f)
			{
				slashGlowShaderIntensity *= 0.82f;
				return;
			}
			Vector2 slashDirection = Projectile.rotation.ToRotationVector2() * Projectile.spriteDirection;
			Vector2 effectPosition = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale * 0.55f);
			float effectRadius = MathHelper.Clamp(Projectile.Size.Length() * Projectile.scale * 1.05f, 145f, 300f);
			float sideWidth = MathHelper.Clamp(Projectile.width * Projectile.scale * 0.48f, 40f, 115f);
			float tailLength = MathHelper.Clamp(Projectile.Size.Length() * Projectile.scale * 0.72f, 85f, 215f);
			float speedFactor = MathHelper.Clamp(progressDelta / 0.45f, 0f, 1f);
			slashGlowShaderIntensity = MathHelper.Clamp(slashGlowShaderIntensity * 0.62f + 0.18f + speedFactor * 0.85f, 0f, 1f);
			CollapsarSlashDistortionSystem.QueueSlash(effectPosition, slashDirection, progressDelta, effectRadius, sideWidth, tailLength);
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
            if (Main.myPlayer == Owner.whoAmI)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<CollapsarLightning>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI, target.whoAmI, Main.rand.Next(100000));
            }
			owner.GetModPlayer<Screenshake_system>().stronger_shake++;
			for (int i = 0; i < 2; i++)
			{
				Vector2 new_vel = Main.MouseWorld - target.Center;
				new_vel.Normalize();
				float size_at_pitch = Main.rand.NextFloat(2.45f, 3.25f) / 2;
				float speed = Main.rand.NextFloat(12f, 18f) / 2;
				new Spark(target.Center, new_vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-75f, 75f))) * speed, Color.White, 300, size_at_pitch, 1f, 0f).Spawn();
			}
            for (int i = 0; i < 5; i++)
			{
				float size_at_pitch = Main.rand.NextFloat(0.25f, 0.75f) / 2;
				float speed = Main.rand.NextFloat(12f, 18f) / 2;
				new Cube_particle(target.Center, Main.rand.NextVector2Circular(16f, 16f), Color.SteelBlue, 300, size_at_pitch, 5f, 0f).Spawn();
			}
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);
        }
	}
}
