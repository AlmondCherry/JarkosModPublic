using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using jarkos.particles;
using jarkos.projectiles.ranged.other;
using jarkos.dusts;
using System.Collections.Generic;
using jarkos.systems;

namespace jarkos.projectiles.mecha
{
	public class Torpedo : ModProjectile
	{
        private const int TrailLength = 25;
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.4f;
        }
        public override void AI()
        {
            Vector2 backPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            trailPositions.Insert(0, backPos);
            if (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.velocity.Length() < 20f)
            {
                Projectile.velocity *= 1.02f;
            }
            float homingRange = 200f;
            NPC target = FindClosestNPC(homingRange);
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), 0.03f);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust engineDust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f, ModContent.DustType<Square_dust>(), -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 0, new Color(50, 150, 255), Main.rand.NextFloat(0.15f, 0.35f));
                    engineDust.noGravity = true;
                    engineDust.fadeIn = 1.2f;
                }
                if (Main.rand.NextBool(3))
                {
                    Dust smoke = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f, DustID.Smoke, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 100, default, 0.8f);
                    smoke.noGravity = true;
                }
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f + 0.4f;
            Lighting.AddLight(Projectile.Center, 0.1f + pulse, 0.4f + pulse, 0.8f + pulse);
            if (Projectile.timeLeft % 30 == 0 && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
            }
        }
        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(50, 150, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(200, 240, 255).ToVector4());
                    List<Vector2> points = new List<Vector2>();
                    Vector2 backPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
                    points.Add(backPos);
                    points.AddRange(trailPositions);
                    TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(16f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/short_burst_sound"), Projectile.Center);
                if (Main.LocalPlayer.Distance(Projectile.Center) < 600f)
                {
                    Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
                }
                float rand_size = Main.rand.NextFloat(0.6f, 1.4f);
                new Circleedge(Projectile.Center, Vector2.Zero, Color.CadetBlue, 180, rand_size, 2f, 0f).Spawn();
                new StarParticle(Projectile.Center, Vector2.Zero, Color.CadetBlue, 180, rand_size * 0.35f, 2f, 0.09f).Spawn();

                for (int i = 0; i < 4; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                    Dust explosion = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Square_dust>(), velocity, 0, new Color(50, 180, 255), Main.rand.NextFloat(0.25f, 0.5f));
                    explosion.noGravity = true;
                    explosion.fadeIn = 1.5f;
                }
                for (int i = 0; i < 5; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                    Dust smoke = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, velocity, 100, default, Main.rand.NextFloat(1.5f, 2f));
                    smoke.noGravity = true;
                }
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i;
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                    Dust ring = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, velocity, 0, new Color(100, 200, 255), 0.2f);
                    ring.noGravity = true;
                }
            }
            Lighting.AddLight(Projectile.Center, 1f, 1.5f, 2f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileType<Rocketmk2_hitbox>(), 200, 0f, Projectile.owner, 0f, 0f);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust Spark = Dust.NewDustPerfect(target.Center, DustID.Electric, Main.rand.NextVector2Circular(5f, 5f), 0, new Color(100, 200, 255), 0.2f);
                    Spark.noGravity = true;
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
