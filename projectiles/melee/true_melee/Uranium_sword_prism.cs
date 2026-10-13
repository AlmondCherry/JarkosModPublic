using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Uranium_sword_prism : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;
		private const int NumAnimationFrames = 5;
		public const int NumBeams = 1;
		public const float MaxCharge = 180f;
		public const float DamageStart = 30f;
		private const float AimResponsiveness = 0.08f;
		private const int SoundInterval = 20;
		private const float MaxManaConsumptionDelay = 15f;
		private const float MinManaConsumptionDelay = 5f;
		private float FrameCounter {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		private float NextManaFrame {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}
		private float ManaConsumptionRate {
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = NumAnimationFrames;
			ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.LastPrism);
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, true);
			FrameCounter += 1f;
			UpdateAnimation();
			PlaySounds();
			UpdatePlayerVisuals(player, rrp);
			if (Projectile.owner == Main.myPlayer) {
				UpdateAim(rrp, player.HeldItem.shootSpeed);
				bool stillInUse = player.channel && !player.noItems && !player.CCed;
				if (stillInUse && FrameCounter == 1f) {
					FireBeams();
				}
				else if (!stillInUse) {
					Projectile.Kill();
				}
			}
			Projectile.timeLeft = 2;
		}
		private void UpdateAnimation() {
			Projectile.frameCounter++;
			int framesPerAnimationUpdate = FrameCounter >= MaxCharge ? 2 : FrameCounter >= (MaxCharge * 0.66f) ? 3 : 4;
			if (Projectile.frameCounter >= framesPerAnimationUpdate) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= NumAnimationFrames) {
					Projectile.frame = 0;
				}
			}
		}
		private void PlaySounds() {
			if (Projectile.soundDelay <= 0) {
				Projectile.soundDelay = SoundInterval;
				if (FrameCounter > 1f) {
					SoundEngine.PlaySound(SoundID.Item15, Projectile.position);
				}
			}
		}
		private void UpdatePlayerVisuals(Player player, Vector2 playerHandPos) {
			Projectile.Center = playerHandPos;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
		}
		private void UpdateAim(Vector2 source, float speed) {
			Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}
			aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, AimResponsiveness));
			aim *= speed;
			if (aim != Projectile.velocity) {
				Projectile.netUpdate = true;
			}
			Projectile.velocity = aim;
		}
		private void FireBeams() {
			Vector2 beamVelocity = Vector2.Normalize(Projectile.velocity);
			if (beamVelocity.HasNaNs()) {
				beamVelocity = -Vector2.UnitY;
			}
			int uuid = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
			int damage = Projectile.damage;
			float knockback = Projectile.knockBack;
			for (int b = 0; b < NumBeams; ++b) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, beamVelocity, ModContent.ProjectileType<Uranium_sword_laser>(), damage, knockback, Projectile.owner, b, uuid);
			}
			Projectile.netUpdate = true;
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			int spriteSheetOffset = frameHeight * Projectile.frame;
			Vector2 sheetInsertPosition = (Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
			Color drawColor = Color.Green;
			Main.EntitySpriteDraw(texture, sheetInsertPosition, new Rectangle?(new Rectangle(0, spriteSheetOffset, texture.Width, frameHeight)), drawColor, Projectile.rotation, new Vector2(texture.Width / 2f, frameHeight / 2f), Projectile.scale, effects, 0f);
			return false;
		}
	}
}
