using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Crystal_sword_proj : ModProjectile
	{
        short[] Array_slime = new short[16];
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		public override void SetDefaults() {
			Projectile.width = 9;
			Projectile.height = 23;
            Projectile.scale = 1f;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 100;
            Projectile.penetrate = 3;
            Projectile.light = 1f;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
             }
        public override void AI() {
            Player player = Main.player[Projectile.owner];
            Projectile.direction = player.direction;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.1f, 0.1f, 0.1f);
            if(Main.rand.NextBool(3)){
            Dust d = Dust.NewDustPerfect(Projectile.Center, 88);
            d.noGravity = true;
            }
            if(Main.rand.NextBool(2)){
            Dust d = Dust.NewDustPerfect(Projectile.Center, 88);
            d.noGravity = true;
            }
            if(Main.rand.NextBool(1)){
            Dust d = Dust.NewDustPerfect(Projectile.Center, 88);
            d.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 45; ++i)
				{
			int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.GemSapphire);
			Main.dust[dust].position = Projectile.Center - new Vector2(2, 2);
            Main.dust[dust].scale = 1.5f;
            Main.dust[dust].noGravity = true;
                }
        }
        public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
