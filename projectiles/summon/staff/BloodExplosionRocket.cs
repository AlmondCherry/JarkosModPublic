using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.summon.staff
{
	public class BloodExplosionRocket : ModProjectile
	{
		private float currentRadius = 0f;
        private float maxRadius = 120f;
        private float expansionSpeed = 15f;
        private bool hasDealtDamage = false;
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 240;
            Projectile.height = 240;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            currentRadius += expansionSpeed;
            expansionSpeed *= 0.9f;
            if (currentRadius > maxRadius)
                currentRadius = maxRadius;
            Projectile.width = (int)(currentRadius * 2);
            Projectile.height = (int)(currentRadius * 2);
            Projectile.Center = Projectile.Center;
            if (!hasDealtDamage && currentRadius > 30f)
            {
                DealAreaDamage();
                hasDealtDamage = true;
            }
            SpawnExplosionParticles();
            float lightIntensity = 1f - (float)Projectile.timeLeft / 30f;
            Lighting.AddLight(Projectile.Center, 1f * (1f - lightIntensity), 0.2f * (1f - lightIntensity), 0.2f * (1f - lightIntensity));
            Projectile.alpha = (int)(255 * ((30f - Projectile.timeLeft) / 30f));
        }
        private void DealAreaDamage()
        {
            float damageRadius = maxRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < damageRadius)
                    {
                        float damageMultiplier = 1f - (distance / damageRadius) * 0.5f;
                        int damage = (int)(Projectile.damage * damageMultiplier);
                        npc.SimpleStrikeNPC(damage, Projectile.Center.X < npc.Center.X ? 1 : -1, false, Projectile.knockBack);
                        Vector2 pushDirection = npc.Center - Projectile.Center;
                        pushDirection.Normalize();
                        npc.velocity += pushDirection * (Projectile.knockBack * 2f);
                    }
                }
            }
        }
        private void SpawnExplosionParticles()
        {
            int particleCount = (int)(currentRadius / 10f);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = currentRadius + Main.rand.NextFloat(-10f, 10f);
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust blood = Dust.NewDustDirect(position, 0, 0, DustID.Blood, (float)Math.Cos(angle) * 3f, (float)Math.Sin(angle) * 3f, 100, default, Main.rand.NextFloat(1f, 2f));
                blood.noGravity = Main.rand.NextBool();
            }
            if (Main.rand.NextBool(2))
            {
                Vector2 randomVel = Main.rand.NextVector2Circular(currentRadius * 0.1f, currentRadius * 0.1f);
                Dust centerDust = Dust.NewDustDirect(Projectile.Center + randomVel, 0, 0, DustID.RedTorch, randomVel.X * 0.5f, randomVel.Y * 0.5f, 0, default, 2f);
                centerDust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
         public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
        }
	}
}
