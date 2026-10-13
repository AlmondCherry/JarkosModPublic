using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;
namespace jarkos.projectiles.melee.true_melee
{
    public class FireGround : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 34;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 210;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
                Projectile.velocity.Y += 0.3f;
            float fade = MathHelper.Clamp(Projectile.timeLeft / 40f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, 1.4f * fade, 0.6f * fade, 0.1f * fade);
            if (Main.netMode != NetmodeID.Server)
            {
                int count = Main.rand.NextBool() ? 2 : 3;
                for (int i = 0; i < count; i++)
                {
                    Vector2 pos = Projectile.Center + new Vector2(
                        Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-4f, 12f));
                    new Fire_particle(
                        pos,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 4f)),
                        new Color(255, 120, 10),
                        lifetime: Main.rand.Next(8, 14),
                        scale: Main.rand.NextFloat(0.4f, 0.7f) * fade,
                        opacity: 0.8f * fade,
                        rotationSpeed: Main.rand.NextFloat(-0.04f, 0.04f)
                    ).Spawn();
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
        }
    }
}
