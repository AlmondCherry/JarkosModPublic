using jarkos.projectiles.mecha;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace Jarkos.projectiles.mecha
{
	public class Scarlet_proj : ModProjectile
	{
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        private ref float TeleportTimer => ref Projectile.ai[0];
        private ref float TeleportCount => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            UpdateTrail();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            SpawnParticles();
            TeleportTimer++;
            if (TeleportTimer >= 15 && TeleportCount < 4)
            {
                PerformWander();
                TeleportTimer = 0;
                TeleportCount++;
            }
            NPC target = FindClosestNPC(300f);
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), 0.03f);
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.RedTorch, Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1, 1), 0, default, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, Main.rand.NextVector2Circular(2, 2), 100, default, 0.8f);
                Spark.noGravity = true;
            }
        }
        private void PerformWander()
        {
            Vector2 oldPos = Projectile.Center;
            float teleportDistance = Main.rand.NextFloat(30f, 50f);
            Vector2 teleportDir = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.3f);
            Projectile.Center += teleportDir * teleportDistance;
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Dust dust = Dust.NewDustPerfect(oldPos, DustID.RedTorch, vel, 0, default, 1.5f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, vel, 0, default, 1.3f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
            trailPositions.Clear();
        }
        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
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
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnShards(target.Center);
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                new Spark(target.Center, sparkVel, new Color(255, 50, 50), 18, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SpawnShards(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, vel, 0, default, 1.5f);
                dust.noGravity = true;
            }
        }
        private void SpawnShards(Vector2 position)
        {
            if (Main.myPlayer != Projectile.owner) return;
            int shardCount = Main.rand.Next(3, 5);
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(6f, 6f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<Scarlet_shard>(), Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawProjectile(lightColor);
            return false;
        }
        private void DrawTrail()
        {
            if (trailPositions.Count < 2) return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                float width = MathHelper.Lerp(8f, 2f, progress);
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Color trailColor = Color.Lerp(new Color(255, 50, 50), new Color(139, 0, 0), progress) * alpha * 0.8f;
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 scale = new Vector2(length, width);
                Main.spriteBatch.Draw(pixel, start, sourceRect, trailColor, rotation, new Vector2(0, 0.5f), scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawProjectile(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            for (int i = 0; i < 4; i++)
            {
                float scale = 1f + i * 0.15f;
                float alpha = 1f - i * 0.25f;
                Color drawColor = Color.Lerp(Color.White, new Color(255, 100, 100), i * 0.3f) * alpha;
                Main.spriteBatch.Draw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale * scale, SpriteEffects.None, 0f);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 200, 200, 200);
        }
	}
}
