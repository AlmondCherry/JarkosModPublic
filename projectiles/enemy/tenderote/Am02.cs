using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using jarkos.particles;
namespace jarkos.projectiles.enemy.tenderote
{
	public class Am02 : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 150;
			Projectile.penetrate = 1;
			Projectile.light = 5f;
		}
        public override void AI()
        {
			Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0f);
			Player player = Main.player[Projectile.owner];
			Projectile.velocity *= 0.97f;
        }
        public override void OnKill(int timeLeft)
        {
            for(int i = 0; i < 10; i++)
            {
            Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
            float size = Main.rand.NextFloat(0.2f, 0.5f);
            new Dust1_particle(Projectile.Center, vct, Color.DarkOrange, 120, size * 3, 1f, 0f).Spawn();
            new Circular_glow(Projectile.Center, vct, Color.Orange, 180, 12f, 1f, 0f).Spawn();
            }
			Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Am02_hitbox>(), 60, 0f, Main.myPlayer);
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/rollback_cannon_shoot"), Projectile.position);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture_edge = ModContent.Request<Texture2D>("Jarkos/textures/circle_edge_texture").Value;
			Vector2 origin = texture_edge.Size() / 2f;
			Color new_color = new Color(255, 0, 0, 2);
            Main.EntitySpriteDraw(texture_edge, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, new_color, 0f, origin, Projectile.scale * 15, SpriteEffects.None, 0);
			return true;
        }
    }
}
