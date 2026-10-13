using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using jarkos.particles;
namespace jarkos.projectiles.enemy.punisher
{
	public class SlashTrap : ModProjectile
	{
		public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.alpha = 50;
            Projectile.light = 0.5f;
            Projectile.scale = 1.25f;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            ++Projectile.frameCounter;
            if (Projectile.frameCounter == 4)
            {
            ++Projectile.frame;
            Projectile.frameCounter = 0;
            }
            if(Projectile.frame > 2)
            {
                Projectile.frame = 0;
            }
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.3f;
            }
            Projectile.scale -= 0.001f;
            if (Projectile.scale < 0.3f)
            {
                Projectile.Kill();
            }
        }
         public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 100, 50, 250);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.OnFire, 180);
        }
    }
}
