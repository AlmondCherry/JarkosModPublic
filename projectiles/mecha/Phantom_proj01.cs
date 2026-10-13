using jarkos;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.mecha
{
	public class Phantom_proj01 : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.DamageType = DamageClass.Default;
            Projectile.scale = 1f;
			Projectile.damage = 100;
            Projectile.penetrate = 1;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
		}
		public override void AI()
		{
			for (int i = 0; i < 1; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(2f, 2f);
				float rand_size = Main.rand.NextFloat(0.25f, 0.85f);
                new Solid_particle(Projectile.Center, vct, Color.White, 200, rand_size, 4f, 0f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<Screenshake_system>();
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/phantom_javelin_impact") with {Pitch = Main.rand.NextFloat(0, 2)}, Projectile.position);
			modPlayer.weak_shake = 1f;
			for (int i = 0; i < 15; i++) {
			Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
			Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemSapphire, speed * 9, Scale: 2f);
			d.noGravity = true;
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
