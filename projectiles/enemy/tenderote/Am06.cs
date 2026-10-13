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
	public class Am06 : ModProjectile
	{
		public int rand_spawn = Main.rand.Next(1, 5);
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
			Projectile.timeLeft = 1;
			Projectile.penetrate = 1;
			Projectile.light = 5f;
		}
        public override void AI()
        {
        }
        public override void OnKill(int timeLeft)
        {
			Player player = Main.player[Projectile.owner];
            for (int i = 0; i < 25; i++) {
			Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, 54, speed * 2.5f, Scale: 1f);
			d.noGravity = false;
        	}
			Vector2 vct = (player.Center - Projectile.Center) * 14;
			Projectile.NewProjectile(null, Projectile.Center, vct * 24, ModContent.ProjectileType<Am06_proj>(), 60, 0f, Main.myPlayer);
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}
