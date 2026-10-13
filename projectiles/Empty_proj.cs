using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles
{
	public class Empty_proj : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.damage = 0;
			Projectile.light = 0f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 1;
		}
        public override void AI()
        {
            Projectile.damage = 0;
        }
    }
}
