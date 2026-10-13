using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Chain_weapon_proj : ModProjectile
	{
		private const string ChainTexturePath = "Jarkos/projectiles/melee/unique_melee/chain_weapon_proj_chain";
		private const string ChainTextureExtraPath = "Jarkos/projectiles/melee/unique_melee/chain_weapon_proj_chain_alt";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.timeLeft = 10;
		}
	private float acceler = 0;
    private int shoottimer = 0;
	public override void AI()
    {
    Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.4f, 0.64f));
    Player player = Main.player[Projectile.owner];
    	player.heldProj = Projectile.whoAmI;
		player.itemTime = 2;
		player.itemAnimation = 2;
    if(PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed)
    {
    	Projectile.timeLeft = 3;
    }
    Projectile.direction = player.direction;
	Vector2 to_cursor = Main.MouseWorld - player.Center;
	Projectile.rotation = to_cursor.ToRotation() + MathHelper.PiOver2;
    player.heldProj = Projectile.whoAmI;
    player.itemTime = player.itemAnimation;
    acceler += 0.9f;
    if(acceler > 290f)
    {
	acceler = 290f;
    }
    Projectile.spriteDirection = Projectile.direction = Terraria.Utils.ToDirectionInt(Projectile.velocity.X > 0.0);
	Vector2 vectorToCursor = Main.MouseWorld - Projectile.Center;
	Projectile.velocity = vectorToCursor * 0.1f;
    }
	public override void OnSpawn(IEntitySource source)
    {
        Projectile.Center = Main.MouseWorld;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.Center = target.Center;
		for (int i = 0; i < 15; i++) {
			Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
			Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Blood, speed, Scale: 1.5f);
			d.noGravity = true;
		}
    }
		#region PreDraw for trail and chain
		public override bool PreDraw(ref Color lightColor) {
			Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
			playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;
			Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(ChainTexturePath);
			Asset<Texture2D> chainTextureExtra = ModContent.Request<Texture2D>(ChainTextureExtraPath);
			Rectangle? chainSourceRectangle = null;
			float chainHeightAdjustment = 0f;
			Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
			Vector2 chainDrawPosition = Projectile.Center;
			Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
			Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
			float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
			if (chainSegmentLength == 0) {
				chainSegmentLength = 10;
			}
			float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
			int chainCount = 0;
			float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;
			while (chainLengthRemainingToDraw > 0f) {
				Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));
				var chainTextureToDraw = chainTexture;
				if (chainCount >= 4) {
				}
				else if (chainCount >= 2) {
					chainTextureToDraw = chainTextureExtra;
					byte minValue = 140;
					if (chainDrawColor.R < minValue)
						chainDrawColor.R = minValue;
					if (chainDrawColor.G < minValue)
						chainDrawColor.G = minValue;
					if (chainDrawColor.B < minValue)
						chainDrawColor.B = minValue;
				}
				else {
					chainTextureToDraw = chainTextureExtra;
					chainDrawColor = Color.White;
				}
				Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);
				chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
				chainCount++;
				chainLengthRemainingToDraw -= chainSegmentLength;
			}
				Texture2D projectileTexture = TextureAssets.Projectile[Projectile.type].Value;
				Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
				SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				for (int k = 0; k < Projectile.oldPos.Length; k++) {
					Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
					Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
					Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
				}
			return true;
		}
		#endregion
	}
}
