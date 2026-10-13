using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.projectiles.bullet;
using Terraria.GameContent;
namespace jarkos.projectiles.ranged.firearm
{
	public class Ultimatum_held : ModProjectile
	{
		public float timer = 0f;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.width = 38;
			Projectile.height = 102;
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
			timer++;
			float chargeRatio = MathHelper.Clamp(Projectile.ai[0] / 20f, 0f, 1f);
			UpdatePlayerVisuals(player, rrp);
			if (Projectile.owner == Main.myPlayer)
			{
				float speedTimesScale = 25f;
				UpdateAim(rrp, speedTimesScale);
				bool StillInUse = player.channel && !player.noItems && !player.CCed;
				if (StillInUse && timer >= 25)
				{
					float speedproj = 1.5f;
					Vector2 projDir = Vector2.Normalize(Projectile.velocity);
					SoundEngine.PlaySound(SoundID.Item158, Projectile.position);
					int damage = player.HeldItem.damage;
					float kb = Projectile.knockBack;
					float ProjectileOffset = 30f;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + ProjectileOffset * projDir, Projectile.velocity * speedproj, ModContent.ProjectileType<Ethereal_bullet>(), damage, kb, Projectile.owner, 6, Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI));
					timer = 0;
					Projectile.netUpdate = true;
				}
				else if (!StillInUse)
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
			Main.EntitySpriteDraw(tex, sheetInsertVec, new Rectangle(0, texYOffset, tex.Width, frameHeight), Color.White, Projectile.rotation, new Vector2(tex.Width / 2f, frameHeight / 2f), Projectile.scale, eff, 0);
			return false;
		}
	}
}
