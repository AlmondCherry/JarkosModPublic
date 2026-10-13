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
	public class HomingTurretRocket_blood : ModProjectile
	{
        private float waveTimer = 0f;
        private float waveAmplitude = 3f;
        private float waveFrequency = 0.15f;
        private Queue<Vector2> trailPositions = new Queue<Vector2>();
        private const int MaxTrailLength = 20;
        private int targetSearchTimer = 0;
        private int currentTarget = -1;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.4f;
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
        }
        public override void AI()
        {
            waveTimer += waveFrequency;
            if (trailPositions.Count >= MaxTrailLength)
                trailPositions.Dequeue();
            trailPositions.Enqueue(Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation();
            FindAndHomeToTarget();
            ApplyWaveMotion();
            AvoidObstacles();
            SpawnTrailParticles();
            if (Projectile.timeLeft % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.position);
            }
        }
        private void FindAndHomeToTarget()
        {
            targetSearchTimer++;
            if (targetSearchTimer >= 5)
            {
                targetSearchTimer = 0;
                currentTarget = FindClosestEnemy(800f);
            }
            if (currentTarget != -1)
            {
                NPC target = Main.npc[currentTarget];
                if (target.active && !target.friendly && target.lifeMax > 5)
                {
                    Vector2 targetDirection = target.Center - Projectile.Center;
                    float distance = targetDirection.Length();
                    if (distance > 0)
                    {
                        targetDirection.Normalize();
                        float homingStrength = MathHelper.Clamp(0.08f + (400f - distance) * 0.0002f, 0.05f, 0.2f);
                        Vector2 currentDirection = Projectile.velocity;
                        currentDirection.Normalize();
                        Vector2 newDirection = Vector2.Lerp(currentDirection, targetDirection, homingStrength);
                        newDirection.Normalize();
                        float speed = Projectile.velocity.Length();
                        Projectile.velocity = newDirection * speed;
                    }
                }
                else
                {
                    currentTarget = -1;
                }
            }
        }
        private int FindClosestEnemy(float maxDistance)
        {
            int closestNPC = -1;
            float closestDistance = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = i;
                    }
                }
            }
            return closestNPC;
        }
        private void ApplyWaveMotion()
        {
            float waveOffset = (float)Math.Sin(waveTimer) * waveAmplitude;
            Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
            perpendicular.Normalize();
            Projectile.velocity += perpendicular * waveOffset * 0.05f;
            float maxSpeed = 16f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
            float minSpeed = 8f;
            if (Projectile.velocity.Length() < minSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= minSpeed;
            }
        }
        private void AvoidObstacles()
        {
            Vector2 checkPosition = Projectile.Center + Projectile.velocity * 5f;
            Point tileCoords = checkPosition.ToTileCoordinates();
            if (WorldGen.InWorld(tileCoords.X, tileCoords.Y))
            {
                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    Vector2[] alternativeDirections = new Vector2[]                    {
                        new Vector2(-Projectile.velocity.Y, Projectile.velocity.X), new Vector2(Projectile.velocity.Y, -Projectile.velocity.X), Projectile.velocity.RotatedBy(MathHelper.ToRadians(45)), Projectile.velocity.RotatedBy(MathHelper.ToRadians(-45)), };
                    foreach (var direction in alternativeDirections)
                    {
                        Vector2 altCheckPos = Projectile.Center + direction * 5f;
                        Point altTileCoords = altCheckPos.ToTileCoordinates();
                        if (WorldGen.InWorld(altTileCoords.X, altTileCoords.Y))
                        {
                            Tile altTile = Main.tile[altTileCoords.X, altTileCoords.Y];
                            if (!altTile.HasTile || !Main.tileSolid[altTile.TileType])
                            {
                                float speed = Projectile.velocity.Length();
                                Vector2 newDir = direction;
                                newDir.Normalize();
                                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), newDir, 0.3f) * speed;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void SpawnTrailParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, -Projectile.velocity.X * 0.3f, -Projectile.velocity.Y * 0.3f, 100, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - Projectile.velocity * 2f, Projectile.width, Projectile.height, DustID.Smoke, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 150, new Color(139, 0, 0, 100), Main.rand.NextFloat(0.5f, 1f));
                smoke.noGravity = true;
            }
            if (Main.rand.NextBool(4))
            {
                Dust glow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch, 0f, 0f, 0, default, 1f);
                glow.noGravity = true;
                glow.velocity = -Projectile.velocity * 0.1f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            CreateBloodExplosion();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
        }
        private void CreateBloodExplosion()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = -0.3f, Volume = 1.2f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f }, Projectile.position);
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BloodExplosionRocket>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
            }
            int dropCount = Main.rand.Next(8, 15);
            for (int i = 0; i < dropCount; i++)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 dropVelocity = Main.rand.NextVector2Circular(8f, 8f);
                    dropVelocity.Y -= Main.rand.NextFloat(2f, 5f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, dropVelocity, ModContent.ProjectileType<BloodDropletRocket>(), Projectile.damage / 3, 0f, Projectile.owner);
                }
            }
            for (int i = 0; i < 50; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                Dust blood = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                blood.noGravity = Main.rand.NextBool();
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust glow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch, velocity.X, velocity.Y, 0, default, 2f);
                glow.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), GoreID.Smoke1 + Main.rand.Next(3));
                gore.velocity *= 0.5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawGlowingProjectile(lightColor);
            return false;
        }
        private void DrawTrail()
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2[] positions = Projectile.oldPos;
            float[] rotations = Projectile.oldRot;
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] == Vector2.Zero)
                    continue;
                float progress = (float)i / positions.Length;
                float scale = MathHelper.Lerp(1f, 0.2f, progress);
                float alpha = MathHelper.Lerp(1f, 0f, progress);
                Color trailColor = new Color(139, 0, 0) * alpha * 0.5f;
                Vector2 drawPos = positions[i] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, rotations[i], texture.Size() / 2f, scale * 0.8f, SpriteEffects.None, 0);
            }
        }
        private void DrawGlowingProjectile(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            for (int i = 0; i < 5; i++)
            {
                float glowScale = 1f + i * 0.15f;
                float glowAlpha = (5 - i) / 15f * pulse;
                Color glowColor = new Color(200, 50, 50) * glowAlpha;
                Main.EntitySpriteDraw(texture, drawPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * glowScale, SpriteEffects.None, 0);
            }
            Color mainColor = Color.Lerp(lightColor, new Color(255, 100, 100), 0.5f);
            Main.EntitySpriteDraw(texture, drawPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 150, 150, 200);
        }
	}
}
