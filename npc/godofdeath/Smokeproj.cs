using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.godofdeath
{
	public class Smokeproj : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 175;
			Projectile.penetrate = -1;
			Projectile.light = 0.25f;
		}
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
			{
				Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Wraith, speed * 2.5f, Scale: 1f);
				d.noGravity = false;
        	}

			SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}
