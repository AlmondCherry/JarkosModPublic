using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.zareth
{
    public class ZarethDeathStone : ModProjectile
    {
        public override string Texture => "jarkos/npc/zareth/ZarethProjOne";
        private ref float Angle => ref Projectile.ai[0];
        private ref float BossIndex => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 16;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(4);
            Projectile.scale = 0f;
        }
        public override void AI()
        {
            int localTime = 180 - Projectile.timeLeft;
            if (Projectile.scale < 1.3f)
                Projectile.scale += 0.1f;
                
            Projectile.rotation += 0.2f;
            NPC boss = Main.npc[(int)BossIndex];
            if (localTime < 60 && boss.active)
            {
                Angle += 0.04f;
                float radius = 90f + (float)Math.Sin(localTime * 0.2f) * 10f;
                Projectile.Center = boss.Center + Angle.ToRotationVector2() * radius;
                Projectile.velocity = Vector2.Zero;
                if (Main.rand.NextBool(6))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Stone, Vector2.Zero, 120, default, 0.9f);
                    d.noGravity = true;
                }
            }
            else if (Projectile.velocity == Vector2.Zero)
            {
                Vector2 outward = Angle.ToRotationVector2();
                Projectile.velocity = outward * Main.rand.NextFloat(8f, 13f) + outward.RotatedBy(1.2f) * 2f;
            }
            else
            {
                Projectile.velocity.Y += 0.12f;
                Projectile.Opacity = Projectile.timeLeft / 60f;
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Stone, -Projectile.velocity * 0.1f, 100, default, 1.1f);
                    d.noGravity = true;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
        }
    }
}
