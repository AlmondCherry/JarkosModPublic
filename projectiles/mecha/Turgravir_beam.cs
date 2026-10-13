using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class Turgravir_beam : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            UpdateTrail();
            Projectile.rotation = Projectile.velocity.ToRotation();
            ApplyGravityPull();
            SpawnParticles();
            Projectile.light = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f;
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        private void ApplyGravityPull()
        {
            float pullRadius = 120f;
            float pullStrength = 0.8f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < pullRadius && distance > 10f)
                    {
                        Vector2 direction = Projectile.Center - npc.Center;
                        direction.Normalize();
                        float force = pullStrength * (1f - distance / pullRadius);
                        npc.velocity += direction * force;
                        if (Main.rand.NextBool(8))
                        {
                            Vector2 dustPos = npc.Center + Main.rand.NextVector2Circular(20f, 20f);
                            Dust gravDust = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, direction * 2f, 0, default, 0.8f);
                            gravDust.noGravity = true;
                        }
                    }
                }
            }
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.GoldFlame, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 0.9f);
                Spark.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawTrail(spriteBatch, pixel);
            DrawGlow(spriteBatch, pixel);
            DrawProjectile(spriteBatch, lightColor);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (trailPositions.Count < 2)
                return;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                float width = (1f - progress) * 6f + 2f;
                Color trailColor = Color.Lerp(new Color(255, 220, 50), new Color(255, 150, 0), progress) * alpha * 0.8f;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                if (length > 0)
                {
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                }
            }
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                float width = (1f - progress) * 3f + 1f;
                Color innerColor = Color.Lerp(Color.White, new Color(255, 255, 150), progress) * alpha * 0.9f;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                if (length > 0)
                {
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), innerColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawGlow(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.3f;
            for (int i = 0; i < 3; i++)
            {
                float scale = (20f + i * 8f) * pulse;
                float alpha = 0.15f - i * 0.04f;
                Color glowColor = new Color(255, 200, 50) * alpha;
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawProjectile(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color coreColor = new Color(255, 255, 200);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), coreColor, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(14f, 6f), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(10f, 4f), SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateGravityExplosion(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center);
        }
        private void CreateGravityExplosion(Vector2 position)
        {
            float explosionRadius = 80f;
            float pullStrength = 4f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(position, npc.Center);
                    if (distance < explosionRadius && distance > 5f)
                    {
                        Vector2 direction = position - npc.Center;
                        direction.Normalize();
                        float force = pullStrength * (1f - distance / explosionRadius);
                        npc.velocity += direction * force;
                    }
                }
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, dustVel, 0, default, 1.5f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f;
                Vector2 velocity = -offset.SafeNormalize(Vector2.Zero) * 2f;
                Dust ring = Dust.NewDustPerfect(position + offset, DustID.YellowTorch, velocity, 0, default, 1.2f);
                ring.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = 0.8f }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, dustVel, 0, default, 1f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
