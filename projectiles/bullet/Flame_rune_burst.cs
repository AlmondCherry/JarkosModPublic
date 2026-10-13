using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using Terraria.Audio;

namespace jarkos.projectiles.bullet
{
    public class Flame_rune_burst : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.tileCollide = false;
            Projectile.width = 750;
            Projectile.height = 750;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.timeLeft = 15;
			Projectile.penetrate = -1;
            Projectile.light = 0.5f;
            Projectile.timeLeft = 10;
        }
        public override void AI()
        {
            Projectile.velocity *= 0f;
            if(Projectile.damage > 300)
            {
                Projectile.damage = 300;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage += (Projectile.damage * 10) / 100;
            for (int i = 0; i < 4; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
                float size = Main.rand.NextFloat(0.4f, 0.8f);
                new Spark(target.Center, vct, Color.IndianRed, 140, size, 2f, 0f).Spawn();
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
                float size = Main.rand.NextFloat(0.4f, 0.8f);
                new Spark(target.Center, vct, Color.IndianRed, 140, size, 2f, 0f).Spawn();
            }
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}
