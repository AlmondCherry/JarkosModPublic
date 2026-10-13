using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
namespace jarkos.projectiles.enemy
{
    public class Rhinobeetle_thread : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WebSpit;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.localAI[1] = Projectile.velocity.ToRotation();
            }
            Projectile.ai[0] += 1f;
            float speed = 8f;
            float waveFreq = 0.2f;
            float waveAmp = 6f;
            Vector2 forward = Projectile.localAI[1].ToRotationVector2();
            Vector2 normal = new Vector2(-forward.Y, forward.X);
            float phaseModifier = Projectile.ai[1] == 0 ? 1f : Projectile.ai[1];
            Projectile.velocity = forward * speed + normal * (float)Math.Cos(Projectile.ai[0] * waveFreq) * waveAmp * phaseModifier;
            Projectile.rotation += 0.2f * phaseModifier;
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Web);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Slow, 120);
        }
    }
}
