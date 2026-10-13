using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Jarkos.projectiles.mecha;
using Terraria.GameContent;

namespace jarkos.projectiles.ranged.firearm
{
	public class Toxic_hawk_swing : ModProjectile
	{
		public float charge = 0f;
		public float charge_max = 50f;
		public float degress_by_attack = 0f;
		public float shoots_counter = 0f;
		public float rotation_timer = 0f;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.width = 50;
			Projectile.height = 36;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.ignoreWater = true;
			Projectile.scale = 1f;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			var Jarkos_player = player.GetModPlayer<Jarkos_player>();
			Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter);
			Projectile.ai[0] += 1f;
			Jarkos_player.nova_charge++;
			charge++;
			degress_by_attack++;
			if (charge > charge_max)
			{
				charge = charge_max;
			}
			float chargeRatio = MathHelper.Clamp(Projectile.ai[0] / 20f, 0f, 1f);
			UpdatePlayerVisuals(player, rrp);
			if (Projectile.owner == Main.myPlayer)
			{
				float speedTimesScale = 25f;
				UpdateAim(rrp, speedTimesScale);
				bool StillInUse = player.channel && !player.noItems && !player.CCed;
				if (StillInUse && charge == charge_max)
				{
					float speedproj = 1.5f;
					Vector2 projDir = Vector2.Normalize(Projectile.velocity);
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/nova_shoot"), Projectile.position);
					int damage = player.HeldItem.damage;
					float kb = Projectile.knockBack;
					float ProjectileOffset = 30f;
					degress_by_attack = 8;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + ProjectileOffset * projDir, Projectile.velocity * speedproj, ModContent.ProjectileType<Railgun_proj>(), damage, kb, Projectile.owner, 6, Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI));
					Projectile.netUpdate = true;
					shoots_counter++;
					charge = 0;
				}
				else if (!StillInUse)
				{
					Projectile.Kill();
				}
				if (shoots_counter == 3)
				{
					rotation_timer = 360f;
					shoots_counter = 0;
				}
				if (rotation_timer > 0f && rotation_timer != 0)
				{
					rotation_timer -= 20;
					Projectile.rotation += MathHelper.ToRadians(rotation_timer);
				}
				if (rotation_timer < 0f)
				{
					rotation_timer = 0;
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
			aimVector = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aimVector, 0.988f));
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
			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction);
			if (Projectile.spriteDirection == -1)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f + (float)Math.PI / degress_by_attack;
			}
			if (Projectile.spriteDirection == 1)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f - (float)Math.PI / degress_by_attack;
			}
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
