using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.GameContent;
using jarkos.particles;
using jarkos.buffs.debuff;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Asteroid_sword_swing : ModProjectile
	{
		public float SWINGRANGE = Math.Sign(Main.rand.NextFloat(-1f, 1f)) * (float)Math.PI;
		private const float FIRSTHALFSWING = 0.45f;
		private const float WINDUP = 0.15f;
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
		private float slowing = 1f;
		private float prepTime => 20f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float impactTime => 20f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 20f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		public override string Texture => "jarkos/items/weapons/melee/true_melee/AsteroidSword";
		private Player Owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 52;
			Projectile.height = 52;
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
			writer.Write(SWINGRANGE);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.spriteDirection = reader.ReadSByte();
			SWINGRANGE = reader.ReadSingle();
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
			Timer++;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
			if (Projectile.spriteDirection > 0)
			{
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(45f);
				effects = SpriteEffects.None;
			}
			else
			{
				origin = new Vector2(Projectile.width, Projectile.height);
				rotationOffset = MathHelper.ToRadians(135f);
				effects = SpriteEffects.FlipHorizontally;
			}
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
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
			Size = MathHelper.SmoothStep(0.85f, 1, Timer / prepTime);
			if (Timer >= prepTime)
			{
				Vector2 vel = Main.MouseWorld - Owner.Center;
				vel.Normalize();
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/asteroid_axe_swing") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
				}
				CurrentStage = AttackStage.Impact;
			}
		}
		private void ImpactStrike()
		{
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / impactTime * slowing);
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
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<Hellfire_debuff>(), 180);
			slowing = 0.1f;
			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 8; i++)
				{
					Vector2 new_vel = new Vector2(Main.rand.NextFloat(-10f, 10f), -12f);
					new Spark_gravit(target.Center, new_vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f))), Color.OrangeRed, 300, Main.rand.NextFloat(0.25f, 0.75f), 2f, 0f).Spawn();
				}
				if (Main.rand.Next(100) < 50)
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/asteroid_axe_impact") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
				}
				else
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/asteroid_axe_impact_two") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
				}
			}
        }
	}
}
