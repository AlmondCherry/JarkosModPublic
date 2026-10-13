using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using Terraria.Audio;

namespace jarkos.projectiles.bullet
{
    public class Feather_black_burst : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
            AIType = ProjectileID.MeteorShot;
            Projectile.tileCollide = false;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.timeLeft = 10;
            Projectile.penetrate = 1;
            Projectile.light = 0.5f;
        }
        public override void AI()
        {
            Projectile.velocity *= 0f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            player.GetModPlayer<Screenshake_system>().regular_shake = 1f;
            player.Heal(damageDone * 3 / 100);
            
            for (int i = 0; i < 3; i++)
            {
                new Circleedge_slimebomb(Projectile.Center, Vector2.Zero, Color.Gray, 95, 0.25f, 1f, 1f).Spawn();
            }
            for (int i = 0; i < 7; i++)
            {
                new StarParticle(Projectile.Center, Main.rand.NextVector2Circular(8f, 8f), Color.Gray, 120, 0.2f, 2f, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
            }
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}
