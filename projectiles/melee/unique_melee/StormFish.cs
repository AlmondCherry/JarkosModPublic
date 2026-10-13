using System;
using jarkos.particles;
using jarkos.particles.storm;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee
{
    public class StormFish : ModProjectile
    {
        private const float HOME_RANGE = 420f;
        private const float SPEED = 10.5f;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 150;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.active && player.whoAmI == Main.myPlayer)
            {
                player.GetModPlayer<PurgatoryPlayer>().AddShake(1.8f, 5);
            }

            SoundEngine.PlaySound(SoundID.Item171 with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.1f, 0.4f) }, target.Center);

            Vector2 hitPoint = target.active ? Vector2.Clamp(Projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight())
                : Projectile.Center;

            Color blueColor = new Color(30, 90, 240);
            Color cyanColor = new Color(90, 210, 255);

            int sparkCount = Main.rand.Next(4, 8);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4.5f, 4.5f);
                Color color = Main.rand.NextBool() ? blueColor : cyanColor;
                float scale = Main.rand.NextFloat(0.4f, 0.8f);
                int lifetime = Main.rand.Next(18, 30);
                new Spark(hitPoint, sparkVel, color, lifetime, scale, 0.85f).Spawn();
            }
        }
        public override void AI()
        {
            if (Projectile.timeLeft < 142)
            {
                NPC target = FindTarget();
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * SPEED;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.085f);
                }
                else if (Projectile.velocity.Length() < SPEED)
                {
                    Projectile.velocity *= 1.02f;
                }
            }
            float wiggle = (float)Math.Sin(Projectile.timeLeft * 0.45f) * 0.18f;
            Projectile.rotation = Projectile.velocity.ToRotation() + wiggle;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.6f + Main.rand.NextVector2Circular(3f, 3f), DustID.Water, -Projectile.velocity * 0.1f, 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                d.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                new Bubble_particle(Projectile.Center - Projectile.velocity * 0.8f, -Projectile.velocity * 0.05f, StormAnchorCharge.ColorCyan, 18, Main.rand.NextFloat(0.2f, 0.3f), 0.55f, 0.03f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 0.1f, 0.25f, 0.45f);
        }
        private NPC FindTarget()
        {
            NPC best = null;
            float bestDist = HOME_RANGE;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy(Projectile))
                    continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }
            return best;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.SplashWeak with { Volume = 0.6f }, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Water, Main.rand.NextVector2Circular(3f, 3f) - new Vector2(0f, 1f), 0, default, Main.rand.NextFloat(1f, 1.6f));
                d.noGravity = Main.rand.NextBool();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;
            var flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically
                : SpriteEffects.None;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, new Color(90, 170, 255, 0) * 0.45f, Projectile.rotation, origin, 1.25f, flip, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, 1f, flip, 0);
            return false;
        }
    }
}
