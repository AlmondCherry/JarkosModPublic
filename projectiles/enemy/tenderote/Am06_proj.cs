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
	public class Am06_proj : ModProjectile
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
			Projectile.timeLeft = 500;
			Projectile.penetrate = 1;
			Projectile.light = 0.25f;
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
			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}
