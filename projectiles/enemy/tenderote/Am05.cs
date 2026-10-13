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
	public class Am05 : ModProjectile
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
			Projectile.timeLeft = 100;
			Projectile.penetrate = 1;
			Projectile.light = 5f;
		}
        public override void AI()
        {
			Lighting.AddLight(Projectile.Center, 0f, 0f, 1f);
			Player player = Main.player[Projectile.owner];
			Projectile.velocity *= 0.96f;
        }
        public override void OnKill(int timeLeft)
        {
			Player player = Main.player[Projectile.owner];
            for (int i = 0; i < 25; i++) {
			Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, 54, speed * 2.5f, Scale: 1f);
			d.noGravity = false;
        	}
			for(int i = 0; i < 2; i++)
			{
			float rand_spawn = Main.rand.NextFloat(-75f, 75f);
			Vector2 rand_spawn_vct = new Vector2(rand_spawn, rand_spawn);
			Vector2 rad_vct = (player.Center - Projectile.Center) * 14;
			Projectile.NewProjectile(null, Projectile.Center + rand_spawn_vct, rad_vct, ModContent.ProjectileType<Am05_smaller>(), 60, 0f, Main.myPlayer);
			}
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}
