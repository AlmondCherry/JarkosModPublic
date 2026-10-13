using System;
using System.IO;
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
	public class Eternal_reign_swing : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/melee/true_melee/Eternal_reign";

		public float SwingTimeTotal = 24f;
		private float SwingArc = MathHelper.Pi * 1.5f;

		private int ComboStep => (int)Projectile.ai[0];
		private int angDir = 1;
		private float aimAngle;
		private int timer;
		private bool spawnedWave;

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 300;
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(aimAngle);
			writer.Write(angDir);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			aimAngle = reader.ReadSingle();
			angDir = reader.ReadInt32();
		}

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];

			float baseSwingTime = 24f;
			float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
			if (meleeSpeedMultiplier < 0.01f) meleeSpeedMultiplier = 0.01f;

			SwingTimeTotal = Math.Max(8f, baseSwingTime / meleeSpeedMultiplier);

			if (Projectile.owner == Main.myPlayer)
			{
				aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
				angDir = (int)Projectile.ai[1] != 0 ? (int)Projectile.ai[1] : 1;
				if (Math.Cos(aimAngle) < 0)
					angDir = -angDir;

				Projectile.netUpdate = true;
			}

			Projectile.rotation = CurrentRotation();
		}

		private float CurrentRotation()
		{
			float t = MathHelper.Clamp(timer / SwingTimeTotal, 0f, 1f);
			float easedT = MathSystem.EaseInOutCubic(t);

			float startAngle = aimAngle - angDir * (SwingArc * 0.5f);
			float endAngle = aimAngle + angDir * (SwingArc * 0.5f);

			return MathHelper.Lerp(startAngle, endAngle, easedT);
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Center = player.MountedCenter;
			Projectile.timeLeft = 2;

			if (timer >= SwingTimeTotal)
			{
				Projectile.Kill();
				return;
			}

			timer++;
			Projectile.rotation = CurrentRotation();


			if (!spawnedWave && timer >= (int)(SwingTimeTotal * 0.35f))
			{
				spawnedWave = true;
				if (Projectile.owner == Main.myPlayer)
				{
					Vector2 launchDir = aimAngle.ToRotationVector2();
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + launchDir * 35f, launchDir * 18f, ModContent.ProjectileType<Eternal_reign_proj>(), Projectile.damage, Projectile.knockBack, player.whoAmI);
					SoundEngine.PlaySound(SoundID.Item84 with { Volume = 0.55f, Pitch = 0.2f }, Projectile.Center);
				}
			}

			HoldPlayer(player);
		}

		private void HoldPlayer(Player player)
		{
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2);
			player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float point = 0f;
			Vector2 dir = Projectile.rotation.ToRotationVector2();
			Vector2 start = Projectile.Center + dir * 16f;
			Vector2 end = Projectile.Center + dir * 70f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 36f, ref point);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D tex = TextureAssets.Projectile[Type].Value;

			Vector2 drawPos = player.MountedCenter - Main.screenPosition;
			bool flip = Math.Cos(aimAngle) < 0;
			SpriteEffects fxFlip = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Vector2 origin = flip ? new Vector2(tex.Width - 6f, tex.Height - 6f)
				: new Vector2(6f, tex.Height - 6f);

			float rotOffset = flip ? MathHelper.Pi * 0.75f : MathHelper.PiOver4;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation + rotOffset, origin, Projectile.scale * 1.15f, fxFlip, 0f);
			return false;
		}
	}
}
