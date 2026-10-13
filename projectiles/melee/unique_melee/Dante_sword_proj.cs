using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Dante_sword_proj : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 130;
			Projectile.height = 130;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
		}
		public enum StagePhase
		{
			Acceler, Slow, Throw, Swing
		}
		private StagePhase CurrentStage
		{
			get => (StagePhase)Projectile.localAI[1];
			set
			{
				Projectile.localAI[1] = (float)value;
			}
		}
		private ref float smooth_timer => ref Projectile.ai[0];
		private ref float size_timer => ref Projectile.ai[1];
		private ref float proj_life => ref Projectile.localAI[0];
		private Player Owner => Main.player[Projectile.owner];
        public override void AI()
        {
			switch (CurrentStage) {
				case StagePhase.Acceler:
					AccelerationSwing();
					break;
				case StagePhase.Slow:
					SlowSwing();
					break;
				case StagePhase.Throw:
					ThrowSword();
					break;
				case StagePhase.Swing:
					CommonSwing();
					break;
			}
			proj_life += 1f;
			float scalefactor = 80f;
			Vector2 relativepoint = Owner.RotatedRelativePoint(Owner.MountedCenter);
			if(Owner.dead)
			{
				Projectile.Kill();
				return;
			}
			Lighting.AddLight(Owner.Center, 0.75f, 0.2f, 0.3f);
			int sign = Math.Sign(Projectile.velocity.X);
			Projectile.velocity = new Vector2(sign, 0f);
			if(proj_life == 1f && Projectile.velocity.X < 0f)
			{
				Projectile.rotation -= MathHelper.PiOver2;
			}
			float rotationValue = Projectile.rotation - MathHelper.PiOver4 * sign;
			Vector2 positionVector = (rotationValue + (sign == -1 ? (float)Math.PI : 0f)).ToRotationVector2() * scalefactor;
			Projectile.position = relativepoint - Projectile.Size / 2f;
			Projectile.position += positionVector;
			Projectile.spriteDirection = Projectile.direction;
			Projectile.timeLeft = 2;
			Owner.ChangeDir(Projectile.direction);
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;
			Owner.itemRotation = MathHelper.WrapAngle(Projectile.rotation);
        }
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}
		private void AccelerationSwing()
		{
			smooth_timer += 1f;
			Projectile.rotation += (float)Math.PI * 2f * MathHelper.Lerp(0, 1, smooth_timer / 12) / 50 * Projectile.spriteDirection;
			if(Projectile.localAI[0] > 50)
			{
			CurrentStage = StagePhase.Slow;
			}
		}
		private void SlowSwing()
		{
			size_timer -= 2f;
			if(smooth_timer > 0)
			{
				smooth_timer -= 1f;
			}
			Projectile.rotation += (float)Math.PI * 2f * MathHelper.Lerp(0, 1, smooth_timer / 12) / 50 * Projectile.spriteDirection;
				if(Projectile.scale > 0.0f)
				{
				Projectile.scale += MathHelper.Lerp(0, 1, size_timer / 12) / 50;
				}
			if(Projectile.localAI[0] > 75)
			{
			Projectile.alpha = 255;
			CurrentStage = StagePhase.Throw;
			}
		}
		private void ThrowSword()
		{
			Vector2 vct = Vector2.Normalize(Main.MouseWorld - Owner.position) * 12f;
			Projectile.NewProjectile(null, Projectile.Center, vct, ModContent.ProjectileType<Dante_sword_disk>(), 300, 0f, Projectile.owner, 0f, 0f);
			CurrentStage = StagePhase.Swing;
		}
		private void CommonSwing()
		{
			smooth_timer += 0.5f;
			size_timer += 3f;
			Projectile.rotation += (float)Math.PI * 2f * MathHelper.Lerp(0, 1, smooth_timer / 24) / 50 * Projectile.spriteDirection;
				if(Projectile.scale < 0.90f && Projectile.localAI[0] > 120)
				{
				Projectile.alpha = 0;
				Projectile.scale += MathHelper.Lerp(0, 1, size_timer / 12) / 50;
				}
			if(Projectile.localAI[0] > 150)
			{
			Projectile.Kill();
			}
		}
		 }
}
