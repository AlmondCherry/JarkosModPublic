using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class AqueousWave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 42;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.965f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = forward.RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < 5; i++)
            {
                float off = Main.rand.NextFloat(-1f, 1f);
                Vector2 pos = Projectile.Center
                    + perp * off * 36f
                    - forward * off * off * 22f;
                Dust d = Dust.NewDustPerfect(pos, DustID.Water,
                    Projectile.velocity * 0.45f, 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(2))
            {
                Dust b = Dust.NewDustPerfect(Projectile.Center + perp * Main.rand.NextFloat(-30f, 30f),
                    DustID.BubbleBurst_Blue, Projectile.velocity * 0.3f, 100, default, 1f);
                b.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.2f, 0.45f, 0.8f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Wet, 240);
        }
    }
}
