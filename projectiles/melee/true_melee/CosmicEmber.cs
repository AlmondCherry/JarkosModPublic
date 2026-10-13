using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.core.particles;
using jarkos.particles.storm;
namespace jarkos.projectiles.melee.true_melee
{
    public class CosmicEmber : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/true_melee/CosmicEmber";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 45;
            Projectile.scale = 1f;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.35f;
            if (Projectile.velocity.Y > 12f)
                Projectile.velocity.Y = 12f;
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
            Projectile.rotation = 0f;
            if (Main.rand.NextBool(3) && Main.netMode != NetmodeID.Server)
            {
                new Fire_particle(
                    Projectile.Center,
                    -Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(120, 60, 255),
                    lifetime: Main.rand.Next(8, 14),
                    scale: Main.rand.NextFloat(0.3f, 0.5f),
                    opacity: 0.7f,
                    rotationSpeed: Main.rand.NextFloat(-0.04f, 0.04f)
                ).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 0.25f, 0.2f, 0.6f);
            Projectile.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 0.6f }, Projectile.Center);
            if (Main.netMode != NetmodeID.Server)
            {
                new SmallExplosionStarParticle(
                    Projectile.Center,
                    Vector2.Zero,
                    new Color(150, 100, 255),
                    lifetime: 16,
                    scale: 0.8f,
                    opacity: 1f,
                    rotationSpeed: Main.rand.NextFloat(-0.05f, 0.05f)
                ).Spawn();
                for (int i = 0; i < 8; i++)
                {
                    new Spark(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(4f, 4f),
                        new Color(100, 150, 255),
                        lifetime: Main.rand.Next(10, 18),
                        scale: Main.rand.NextFloat(0.4f, 0.7f),
                        opacity: 0.9f
                    ).Spawn();
                }
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.3f, 0.9f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
    }
}
