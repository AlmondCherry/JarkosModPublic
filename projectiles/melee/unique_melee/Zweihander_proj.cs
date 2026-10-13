using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Zweihander_proj : ModProjectile {
		public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.ownerHitCheck = false;
			Projectile.scale = 1f;
			Projectile.timeLeft = 300;
			Projectile.light = 0.95f;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float size = Main.rand.NextFloat(0.75f, 0.85f);
			new StarParticleBigger(target.Center, Vector2.Zero, Color.Yellow, 100, size / 2f, 0.75f, 0.1f).Spawn();
        }
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			Projectile.velocity *= .9998f;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			NPChoming.HomingProj(Projectile, 750f, 1.5f, 290);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, Color.Yellow, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
