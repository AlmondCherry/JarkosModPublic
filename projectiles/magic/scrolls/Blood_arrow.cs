using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using jarkos.systems;
using Terraria.ModLoader.Default;
using Terraria.Audio;
namespace jarkos.projectiles.magic.scrolls
{
    internal class Blood_arrow : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 300;
            Projectile.scale = 1;
        }
        public override void OnKill(int timeLeft)
			  {
            for (int a = 0; a < 20; a++) {
			      Vector2 speed = Main.rand.NextVector2CircularEdge(6f,6f);
			      Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Blood, null, Scale: 2f);
			      d.noGravity = false;
            d.velocity.Y += 0.2f;
            }
			      SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/smallslime_impact"), Projectile.position);
			  }
        public override void AI()
        {
        Terraria.Player player = Main.player[Projectile.owner];
        Projectile.direction = player.direction;
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (Main.rand.NextBool(1))
            {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Blood, Scale: 0.75f);
            dust.velocity /= 10;
            dust.noGravity = true;
            }
         if (Main.rand.NextBool(1))
            {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Blood, Scale: 1.25f);
            dust.velocity /= 10;
            dust.noGravity = true;
            }
         if (Main.rand.NextBool(1))
            {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Blood, Scale: 0.75f);
            dust.velocity /= 10;
            dust.noGravity = true;
            }
        }
    }
}
