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
namespace jarkos.projectiles.melee.unique_melee
{
	public class Mirror_swing : ModProjectile
	{
		private const float swingrange = 1.67f * (float)Math.PI;
		private const float halfofswing = 0.45f;
		private const float unwind = 0.4f;
		private const float windup = 0.15f;
		private enum Stages
		{
			start, impact, end
		}
		private Stages currentstage {
			get => (Stages)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				Timer = 0;
			}
		}
		private ref float Timer => ref Projectile.ai[2];
		private ref float StartAngle => ref Projectile.ai[1];
		private ref float Progress => ref Projectile.localAI[1];
		private ref float Size => ref Projectile.localAI[2];
		private float startspeed => 12f / owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float impactspeed => 12f / owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float endspeed => 12f / owner.GetTotalAttackSpeed(Projectile.DamageType);
		public override string Texture => "jarkos/items/weapons/melee/unique_melee/Sunfate";
		private Player owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
        public override void SetDefaults()
        {
            Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.timeLeft = 10000;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
        }
		public override void OnSpawn(IEntitySource source) {
			Projectile.spriteDirection = Main.MouseWorld.X > owner.MountedCenter.X ? 1 : -1;
			float targetAngle = (Main.MouseWorld - owner.MountedCenter).ToRotation();
			StartAngle = targetAngle - halfofswing * swingrange * Projectile.spriteDirection;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((sbyte)Projectile.spriteDirection);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.spriteDirection = reader.ReadSByte();
		}
        public override void AI()
        {
			owner.itemAnimation = 2;
			owner.itemTime = 2;
			if (!owner.active || owner.dead || owner.noItems || owner.CCed) {
				Projectile.Kill();
				return;
			}
			switch(currentstage)
			{
				case Stages.start:
				startmove();
				break;
				case Stages.impact:
				impactmove();
				break;
				default:
				endmove();
				break;
			}
			swordpos();
			Timer++;
        }
		public override bool PreDraw(ref Color lightColor) {
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;
			if (Projectile.spriteDirection > 0) {
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(45f);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(Projectile.width, Projectile.height);
				rotationOffset = MathHelper.ToRadians(135f);
				effects = SpriteEffects.FlipHorizontally;
			}
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 start = owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}
		public override void CutTiles() {
			Vector2 start = owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Terraria.Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool? CanDamage() {
			if (currentstage == Stages.start)
				return false;
			return base.CanDamage();
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = target.position.X > owner.MountedCenter.X ? 1 : -1;
		}
		private void swordpos()
		{
			Projectile.rotation = StartAngle + Projectile.spriteDirection * Progress;
			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
			Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2);
			armPosition.Y += owner.gfxOffY;
			Projectile.Center = armPosition;
			Projectile.scale = Size * 1.89f * owner.GetAdjustedItemScale(owner.HeldItem);
			owner.heldProj = Projectile.whoAmI;
		}
		private void startmove()
		{
			Progress = windup * swingrange * (1f - Timer / startspeed);
			if (Timer >= startspeed) {
				SoundEngine.PlaySound(SoundID.Item1);
				currentstage = Stages.impact;
			}
		}
		private void impactmove()
		{
			Progress = MathHelper.SmoothStep(0, swingrange, (1 - unwind) * Timer / impactspeed);
			if(Timer >= impactspeed) {
				SoundEngine.PlaySound(SoundID.Item1);
				currentstage = Stages.end;
			}
		}
		private void endmove()
		{
			Progress = MathHelper.SmoothStep(0, swingrange, (1 - unwind) + unwind * Timer / endspeed);
			if(Timer >= endspeed) {
				Projectile.Kill();
			}
		}
    }
}
