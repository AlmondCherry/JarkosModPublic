using jarkos.buffs.summonbuff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon
{
    public class AstralDagger : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/summon/staff/PhantomDagger/PhantomDagger_proj";
        private const int HomingStartFrame = 4;
        private const float HomingSpeed = 16f;
        private const float HomingSmoothing = 0.18f;
        private const int PoisonTicks = 60 * 5;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.ai[1] += 1f;
            int targetIdx = (int)Projectile.ai[0] - 1;
            
            if (Projectile.ai[1] >= HomingStartFrame && targetIdx >= 0 && targetIdx < Main.maxNPCs)
            {
                NPC npc = Main.npc[targetIdx];
                if (npc.active && !npc.friendly && npc.life > 0)
                {
                    Vector2 toTarget = npc.Center - Projectile.Center;
                    if (toTarget.LengthSquared() > 0.01f)
                    {
                        Vector2 desired = Vector2.Normalize(toTarget) * HomingSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingSmoothing);
                    }
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, 0.35f, 0.55f, 0.95f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<AstralPoison>(), PoisonTicks);
        }
    }
}
