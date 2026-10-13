using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.ranged.firearm
{
	public class Nox_explosion_eff : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = true;
			Projectile.scale = 1f;
			Projectile.timeLeft = 25;
			Projectile.scale = 1f;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 0.10f;
			Projectile.aiStyle = -1;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			Projectile.velocity *= 0f;
			if(Projectile.timeLeft < 25)
			{
				Projectile.alpha += 10;
				Projectile.scale += 0.1f;
				if(Projectile.alpha > 250)
				{
					Projectile.Kill();
				}
			}
		}
	 	public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Color color26 = Color.Cyan;
            color26 = Projectile.GetAlpha(color26);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
	}
}
