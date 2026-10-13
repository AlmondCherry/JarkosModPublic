using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
namespace jarkos.projectiles.enemy.tenderote
{
	public class Am03 : ModProjectile
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
			Projectile.timeLeft = 35;
			Projectile.penetrate = 1;
			Projectile.light = 5f;
		}
        public override void AI()
        {
			Lighting.AddLight(Projectile.Center, 0f, 1f, 0.1f);
			Player player = Main.player[Projectile.owner];
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 25; i++) {
			Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, 54, speed * 2.5f, Scale: 1f);
			d.noGravity = false;
        	}
			for(int i = 0; i < 6; i++)
			{
			Vector2 rad_vct = Main.rand.NextVector2Circular(4f, 4f);
			Projectile.NewProjectile(null, Projectile.Center, rad_vct, ModContent.ProjectileType<Am03_bigger_spore_smoke>(), 60, 0f, Main.myPlayer);
			}
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
			for(int i = 0; i < 14; i++)
			{
			Vector2 rad_vct = Main.rand.NextVector2Circular(4f, 4f);
			Projectile.NewProjectile(null, Projectile.Center, rad_vct, ModContent.ProjectileType<Am03_spore_smoke>(), 60, 0f, Main.myPlayer);
			}
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}
