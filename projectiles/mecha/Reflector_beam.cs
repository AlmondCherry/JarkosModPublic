using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public class Reflector_beam : ModProjectile
    {
        private const int MaxReflections = 5;
        private const int TrailLength = 30;
        private const float SearchRadius = 600f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<int> hitEnemyIDs = new List<int>();
        public int ReflectionCount
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float GlowIntensity
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = MaxReflections + 1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            GlowIntensity = 1f;
        }
        public override void AI()
        {
            UpdateTrail();
            Projectile.rotation = Projectile.velocity.ToRotation();
            GlowIntensity = 1f + ReflectionCount * 0.4f;
            Color currentColor = GetCurrentColor();
            float lightMult = 0.5f + ReflectionCount * 0.15f;
            Lighting.AddLight(Projectile.Center, currentColor.ToVector3() * lightMult);
            SpawnTrailParticles();
            Projectile.scale = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f + ReflectionCount * 0.15f;
        }
        private int FindNewTarget()
        {
            float closestDistance = SearchRadius;
            int closestNPC = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                if (hitEnemyIDs.Contains(i))
                    continue;
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < closestDistance)
                {
                    if (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        closestDistance = distance;
                        closestNPC = i;
                    }
                }
            }
            return closestNPC;
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
        }
        private Color GetCurrentColor()
        {
            float progress = ReflectionCount / (float)MaxReflections;
            Color startColor = new Color(50, 200, 255);
            Color midColor = new Color(200, 100, 255);
            Color endColor = new Color(255, 50, 50);
            if (progress < 0.5f)
                return Color.Lerp(startColor, midColor, progress * 2f);
            else
                return Color.Lerp(midColor, endColor, (progress - 0.5f) * 2f);
        }
        private void SpawnTrailParticles()
        {
            if (Main.rand.NextBool(3))
            {
                Color color = GetCurrentColor();
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.RainbowTorch, Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1, 1), 0, color, 0.8f + ReflectionCount * 0.2f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (ReflectionCount >= MaxReflections)
            {
                CreateExplosion();
                return true;
            }
            ReflectionCount++;
            Projectile.damage = (int)(Projectile.damage * 1.25f);
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;
            Projectile.velocity *= 1.05f;
            CreateReflectionEffects();
            PlayReflectionSound();
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hitEnemyIDs.Contains(target.whoAmI))
            {
                hitEnemyIDs.Add(target.whoAmI);
            }
            if (ReflectionCount >= MaxReflections)
            {
                CreateExplosion();
                Projectile.Kill();
                return;
            }
            ReflectionCount++;
            Projectile.damage = (int)(Projectile.damage * 1.25f);
            int nextTarget = FindNewTarget();
            float speed = Projectile.velocity.Length() * 1.1f;
            if (nextTarget >= 0)
            {
                NPC nextNPC = Main.npc[nextTarget];
                Vector2 direction = (nextNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = direction * speed;
                CreateRedirectEffect(nextNPC.Center);
            }
            else
            {
                Vector2 awayDir = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX);
                awayDir = awayDir.RotatedByRandom(MathHelper.ToRadians(30));
                Projectile.velocity = awayDir * speed;
            }
            CreateReflectionEffects();
            PlayReflectionSound();
        }
        private void CreateRedirectEffect(Vector2 targetPos)
        {
            Color color = GetCurrentColor();
            Vector2 direction = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
            float distance = Vector2.Distance(Projectile.Center, targetPos);
            int dustCount = (int)(distance / 25f);
            for (int i = 0; i < dustCount; i++)
            {
                float progress = i / (float)dustCount;
                Vector2 pos = Vector2.Lerp(Projectile.Center, targetPos, progress);
                Dust line = Dust.NewDustPerfect(pos, DustID.RainbowTorch, Vector2.Zero, 0, color * (1f - progress), 0.6f);
                line.noGravity = true;
                line.fadeIn = 0.3f;
            }
        }
        private void PlayReflectionSound()
        {
            float pitch = -0.3f + ReflectionCount * 0.15f;
            SoundEngine.PlaySound(SoundID.Item158 with { Pitch = pitch, Volume = 0.7f }, Projectile.Center);
        }
        private void CreateReflectionEffects()
        {
            Color color = GetCurrentColor();
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.5f, 1.2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, dustVel, 0, color, 1.2f);
                dust.noGravity = true;
            }
        }
        private void CreateExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.2f }, Projectile.Center);
            int explosionDamage = Projectile.damage * 2;
            float explosionRadius = 120f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < explosionRadius)
                    {
                        float damageMult = 1f - (dist / explosionRadius) * 0.5f;
                        int finalDamage = (int)(explosionDamage * damageMult);
                        npc.SimpleStrikeNPC(finalDamage, Projectile.direction, true, 8f, DamageClass.Ranged);
                    }
                }
            }
            Color finalColor = GetCurrentColor();
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(12, 12) * Main.rand.NextFloat(0.3f, 1f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, finalColor, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            for (int ring = 0; ring < 3; ring++)
            {
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi / 24 * i;
                    Vector2 vel = angle.ToRotationVector2() * (5 + ring * 3);
                    Dust ringDust = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * ring * 10, DustID.RainbowTorch, vel, 0, Color.Lerp(finalColor, Color.White, ring * 0.2f), 1.8f - ring * 0.3f);
                    ringDust.noGravity = true;
                }
            }
            foreach (int enemyID in hitEnemyIDs)
            {
                if (enemyID >= 0 && enemyID < Main.maxNPCs && Main.npc[enemyID].active)
                {
                    Main.npc[enemyID].SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0, DamageClass.Ranged);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color beamColor = GetCurrentColor();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            DrawTrail(spriteBatch, pixel, beamColor);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            DrawCore(spriteBatch, pixel, beamColor);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel, Color baseColor)
        {
            if (trailPositions.Count < 2)
                return;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = i / (float)trailPositions.Count;
                float alpha = (1f - progress) * 0.3f;
                float width = (1f - progress) * (16 + ReflectionCount * 4);
                Color glowColor = baseColor * alpha * GlowIntensity;
                glowColor.A = 0;
                DrawLine(spriteBatch, pixel, start, end, glowColor, width);
            }
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = i / (float)trailPositions.Count;
                float alpha = (1f - progress) * 0.9f;
                float width = (1f - progress) * (4 + ReflectionCount);
                Color coreColor = Color.Lerp(Color.White, baseColor, 0.2f) * alpha;
                DrawLine(spriteBatch, pixel, start, end, coreColor, width);
            }
        }
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 1f) return;
            float rotation = diff.ToRotation();
            Vector2 scale = new Vector2(length, width);
            spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), scale, SpriteEffects.None, 0f);
        }
        private void DrawCore(SpriteBatch spriteBatch, Texture2D pixel, Color baseColor)
        {
            Texture2D glowTexture2 = ModContent.Request<Texture2D>("jarkos/projectiles/bullet/Biomechanical_heart_proj_glowtwo").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = new Vector2(glowTexture2.Width / 2f, glowTexture2.Height / 2f);
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.2f;
            for (int i = 4; i >= 1; i--)
            {
                float size = (12 + ReflectionCount * 3 + i * 6) * pulse * Projectile.scale;
                Color glowColor = baseColor * (0.15f / i) * GlowIntensity;
                glowColor.A = 0;
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f), size, SpriteEffects.None, 0f);
            }
            float coreSize = (1 + ReflectionCount * 0.1f) * pulse * Projectile.scale * 0.25f;
            Color coreColor = Color.Lerp(Color.White, baseColor, 0.3f);
            spriteBatch.Draw(glowTexture2, drawPos, null, coreColor, 0f, glowOrigin, coreSize, SpriteEffects.None, 0f);
        }
        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;
    }
}
