using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.projectiles.bullet;
using Terraria.GameContent;

namespace jarkos.projectiles.ranged.bow
{
	public class Mechanical_bow_swing : ModProjectile
	{
		public float shoot_timer = 0f;
		public float reload_shoot = 0f;
		public const int NumBeams = 6;
		public const float MaxCharge = 20f;
		public const float DamageStart = 30f;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 5;
		}
		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 26;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.ignoreWater = true;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter);

			Projectile.damage = 32;
			Projectile.ai[0] += 1f;

			shoot_timer++;
			reload_shoot++;

			if(reload_shoot > 15)
			{
				reload_shoot = 0;
			}

			++Projectile.frameCounter;

			if(Projectile.frameCounter > 8 && reload_shoot > 5)
			{
				Projectile.frame = 0;
				Projectile.frameCounter = 0;
			}

			if (reload_shoot > 5)
			{
				if(Projectile.frameCounter == 2 || Projectile.frameCounter == 4 || Projectile.frameCounter == 6 || Projectile.frameCounter == 8  || Projectile.frameCounter == 10)
				{
				++Projectile.frame;
				}

			}
			float chargeRatio = MathHelper.Clamp(Projectile.ai[0] / 20f, 0f, 1f);

			if (Projectile.soundDelay <= 0)
			{
				Projectile.soundDelay = 20;
				if (Projectile.ai[0] > 1f)
				{
					SoundEngine.PlaySound(SoundID.Item15, Projectile.position);
				}
			}

			UpdatePlayerVisuals(player, rrp);

			if (Projectile.owner == Main.myPlayer)
			{
				float speedTimesScale = 10f;
				UpdateAim(rrp, speedTimesScale);
				bool crystalStillInUse = player.channel && !player.noItems && !player.CCed;

				if (crystalStillInUse && reload_shoot == 15)
				{
					Item weapon = player.inventory[player.selectedItem];
					int arrowType = ProjectileID.WoodenArrowFriendly;
					bool canShoot = true;
					if (weapon.useAmmo > 0)
					{
						float speed = 0f;
						canShoot = player.PickAmmo(weapon, out arrowType, out speed, out int ammoDamage, out float ammoKb, out _);
					}
					if (!canShoot)
					{
						Projectile.Kill();
						return;
					}

					SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
					Vector2 beamVelocity = Vector2.Normalize(Projectile.velocity);
					if (beamVelocity.HasNaNs())
					{
						beamVelocity = -Vector2.UnitY;
					}
					int damage = Projectile.damage;
					float kb = Projectile.knockBack;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, Projectile.velocity.X, Projectile.velocity.Y, ModContent.ProjectileType<Mechanical_arrow>(), damage, kb, Projectile.owner, 6, Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI));
					Projectile.netUpdate = true;
				}
				else if (!crystalStillInUse)
				{
					Projectile.Kill();
				}
			}
			
			Projectile.timeLeft = 2;
		}
		private void UpdateAim(Vector2 source, float speed)
		{
			Vector2 aimVector = Vector2.Normalize(Main.MouseWorld - source);
			if (aimVector.HasNaNs())
			{
				aimVector = -Vector2.UnitY;
			}
			aimVector = Vector2.Normalize(Vector2.Lerp(aimVector, Vector2.Normalize(Projectile.velocity), 0.89f));
			aimVector *= speed;
			if (aimVector != Projectile.velocity)
			{
				Projectile.netUpdate = true;
			}
			Projectile.velocity = aimVector;
		}
		private void UpdatePlayerVisuals(Player player, Vector2 rrp)
		{
			Projectile.Center = rrp;
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			SpriteEffects eff = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
			int texYOffset = frameHeight * Projectile.frame;
			Vector2 sheetInsertVec = (Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
			Main.EntitySpriteDraw(tex, sheetInsertVec, new Rectangle(0, texYOffset, tex.Width, frameHeight), Color.White, Projectile.rotation, new Vector2((float)tex.Width / 2f, (float)frameHeight / 2f), Projectile.scale, eff, 0);
			return false;
		}
	}
}
