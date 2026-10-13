using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Arrow_death : ModProjectile
	{
        private const float DETECTION_RANGE = 350f;
        private const float MIN_FLIGHT_TIME = 10f;
        private bool hasTransformed = false;
        private float flightTime = 0f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int TRAIL_LENGTH = 8;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            flightTime++;
            UpdateTrail();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.08f;
            CreateAmbientParticles();
            Lighting.AddLight(Projectile.Center, 0.2f, 0.6f, 0.9f);

            if (flightTime > MIN_FLIGHT_TIME && !hasTransformed)
            {
                List<NPC> triggerTargets = FindEnemiesInRange(DETECTION_RANGE, 1);
                if (triggerTargets.Count > 0)
                {
                    List<NPC> actualTargets = FindEnemiesInRange(750f, 4);
                    TransformToLightning(actualTargets);
                }
            }
        }

        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            while (trailPositions.Count > TRAIL_LENGTH)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }

        private void CreateAmbientParticles()
        {
            if (Main.rand.NextBool(3))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.Electric, Projectile.velocity * 0.1f, 0, Color.Cyan, Main.rand.NextFloat(0.4f, 0.7f));
                spark.noGravity = true;
            }
        }

        private List<NPC> FindEnemiesInRange(float range, int maxTargets)
        {
            List<NPC> targets = new List<NPC>();
            Vector2 direction = Vector2.Normalize(Projectile.velocity);
            List<(NPC npc, float dist)> list = new List<(NPC, float)>();
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist >= range)
                    continue;
                Vector2 toEnemy = Vector2.Normalize(npc.Center - Projectile.Center);
                float dot = Vector2.Dot(direction, toEnemy);
                if (dot > -0.2f)
                {
                    if (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        list.Add((npc, dist));
                    }
                }
            }
            list.Sort((a, b) => a.dist.CompareTo(b.dist));
            for (int i = 0; i < Math.Min(list.Count, maxTargets); i++)
            {
                targets.Add(list[i].npc);
            }
            return targets;
        }

        private void TransformToLightning(List<NPC> targets)
        {
            hasTransformed = true;
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = 0.5f }, Projectile.Center);
            CreateDissolveEffect();
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    NPC target = targets[i];
                    float multiplier = MathHelper.Max(0f, 1f - i * 0.25f);
                    int scaledDamage = (int)(Projectile.damage * multiplier);
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FulguriteLightning>(), scaledDamage, Projectile.knockBack * multiplier, Projectile.owner, ai0: target.whoAmI, ai1: Projectile.Center.X, ai2: Projectile.Center.Y);
                }
            }
            Projectile.Kill();
        }

        private void CreateDissolveEffect()
        {
            Vector2 pos = Projectile.Center;
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust electric = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8, 8), DustID.Electric, vel, 0, Color.Cyan, Main.rand.NextFloat(0.3f, 0.6f));
                electric.noGravity = true;
            }
            new Circular_glow(pos, Vector2.Zero, new Color(40, 180, 255), 16, 0.5f, 0.9f).Spawn();
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.8f }, pos);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawArrowWithGlow(lightColor);
            return false;
        }

        private void DrawTrail()
        {
            if (trailPositions.Count < 2) return;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float t0 = (float)i / trailPositions.Count;
                Vector2 start = trailPositions[i];
                Vector2 end = trailPositions[i + 1];

                float glowWidth = MathHelper.Lerp(14f, 1f, t0);
                float coreWidth = MathHelper.Lerp(4f, 0.5f, t0);

                Color glowColor = new Color(20, 120, 255) * ((1f - t0) * 0.4f);
                Color coreColor = Color.Lerp(Color.White, new Color(60, 180, 255), t0) * ((1f - t0) * 0.8f);

                DrawSegmentLine(sb, pixel, start, end, glowWidth, glowColor);
                DrawSegmentLine(sb, pixel, start, end, coreWidth, coreColor);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawArrowWithGlow(Color lightColor)
        {
            Texture2D arrowTexture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = arrowTexture.Size() * 0.5f;

            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;
            Color cyanGlow = new Color(30, 160, 255) * 0.5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glowTex, drawPos, null, cyanGlow, 0f, glowOrigin, 0.4f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(arrowTexture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            if (hasTransformed) return;
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, Color.Cyan, Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
            }
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 0.1f) return;
            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }

	public class FulguriteLightning : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/bullet/Bloody_bolt_impulse";
        private List<Vector2> mainBoltPoints = new List<Vector2>();
        private const int BOLT_SEGMENTS = 8;
        private const float JAGGEDNESS = 24f;
        private bool initialized = false;
        private int animationTimer = 0;
        private const int ANIMATION_DURATION = 24;
        private bool hasDealtDamage = false;

        private int TargetNPC => (int)Projectile.ai[0];
        private Vector2 StartPosition => new Vector2(Projectile.ai[1], Projectile.ai[2]);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ANIMATION_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (!initialized)
            {
                InitializeLightning();
                initialized = true;
            }

            animationTimer++;

            if (Main.rand.NextBool(4) && mainBoltPoints.Count > 0)
            {
                Vector2 pos = mainBoltPoints[Main.rand.Next(mainBoltPoints.Count)];
                Dust electric = Dust.NewDustPerfect(pos, DustID.Electric, Vector2.Zero, 0, Color.Cyan, 0.4f);
                electric.noGravity = true;
            }

            if (!hasDealtDamage && animationTimer >= 5)
            {
                DealDamageToTarget();
                hasDealtDamage = true;
            }
        }

        private void InitializeLightning()
        {
            NPC target = Main.npc[TargetNPC];
            Vector2 end = target.active ? target.Center : (StartPosition + Vector2.UnitY * 300f);
            Vector2 start = StartPosition;

            mainBoltPoints = GenerateLightningBolt(start, end, BOLT_SEGMENTS, JAGGEDNESS);
            Projectile.Center = (start + end) / 2f;

            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.1f }, Projectile.Center);
        }

        private List<Vector2> GenerateLightningBolt(Vector2 start, Vector2 end, int segments, float jaggedness)
        {
            List<Vector2> points = new List<Vector2>(segments + 1);
            points.Add(start);
            Vector2 direction = end - start;
            Vector2 normalDir = Vector2.Normalize(direction);
            Vector2 perpendicular = new Vector2(-normalDir.Y, normalDir.X);

            for (int i = 1; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePos = start + direction * progress;
                float offset = Main.rand.NextFloat(-jaggedness, jaggedness);
                float envelopeFactor = (float)Math.Sin(progress * Math.PI);
                offset *= envelopeFactor;
                Vector2 point = basePos + perpendicular * offset;
                points.Add(point);
            }
            points.Add(end);
            return points;
        }

        private void DealDamageToTarget()
        {
            NPC target = Main.npc[TargetNPC];
            if (!target.active) return;
            Vector2 pos = target.Center;
            new Circular_glow(pos, Vector2.Zero, new Color(40, 180, 255), 18, 0.6f, 0.95f).Spawn();

            int count = Main.rand.Next(2, 4);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + (i / (float)count) * MathHelper.TwoPi + Main.rand.NextFloat(-0.35f, 0.35f);
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(5.5f, 9.5f);
                float scale = Main.rand.NextFloat(0.22f, 0.38f);
                int lifetime = Main.rand.Next(18, 30);
                Color cyanColor = new Color(40, 180, 255);
                new Electric_particle(pos, particleVel, cyanColor, lifetime, scale, 1f, 0.92f).Spawn();
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (mainBoltPoints.Count < 2) return false;
            for (int i = 0; i < mainBoltPoints.Count - 1; i++)
            {
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), mainBoltPoints[i], mainBoltPoints[i + 1], 16f, ref point))
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int count = Main.rand.Next(1, 2);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + (i / (float)count) * MathHelper.TwoPi + Main.rand.NextFloat(-0.35f, 0.35f);
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(5.5f, 9.5f);
                float scale = Main.rand.NextFloat(0.22f, 0.38f);
                int lifetime = Main.rand.Next(18, 30);
                Color cyanColor = new Color(40, 180, 255);
                new Electric_particle(target.Center, particleVel, cyanColor, lifetime, scale, 1f, 0.92f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (mainBoltPoints.Count < 2) return false;

            float buildProgress = MathHelper.Clamp(animationTimer / 5f, 0.1f, 1f);
            float fadeFactor = 1f - MathHelper.Clamp((animationTimer - 5f) / (ANIMATION_DURATION - 5f), 0f, 1f);
            float easedFade = fadeFactor * fadeFactor;

            if (easedFade <= 0.01f) return false;

            int activeCount = (int)MathF.Ceiling(mainBoltPoints.Count * buildProgress);
            if (activeCount < 2) activeCount = 2;
            if (activeCount > mainBoltPoints.Count) activeCount = mainBoltPoints.Count;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 startDrawPos = mainBoltPoints[0] - Main.screenPosition;
            Vector2 endDrawPos = mainBoltPoints[activeCount - 1] - Main.screenPosition;

            sb.Draw(glowTex, startDrawPos, null, new Color(30, 130, 255) * (0.6f * easedFade), 0f, glowOrigin, 0.6f * easedFade, SpriteEffects.None, 0f);
            sb.Draw(glowTex, endDrawPos, null, new Color(230, 250, 255) * (0.75f * easedFade), 0f, glowOrigin, 0.45f * easedFade, SpriteEffects.None, 0f);

            for (int i = 0; i < activeCount - 1; i++)
            {
                Vector2 start = mainBoltPoints[i];
                Vector2 end = mainBoltPoints[i + 1];

                float t0 = (float)i / (mainBoltPoints.Count - 1);
                float t1 = (float)(i + 1) / (mainBoltPoints.Count - 1);
                float tAvg = (t0 + t1) * 0.5f;

                float outerWidth = MathHelper.Lerp(34f, 5f, tAvg) * easedFade;
                float coreWidth  = MathHelper.Lerp(7.5f, 1.8f, tAvg) * easedFade;
                float innerWidth = MathHelper.Lerp(3.5f, 0.9f, tAvg) * easedFade;

                Color outerColor = Color.Lerp(new Color(20, 100, 255), new Color(160, 210, 255), tAvg) * (0.35f * easedFade);
                Color coreColor  = Color.Lerp(new Color(50, 160, 255), Color.White, tAvg) * (0.85f * easedFade);
                Color innerColor = Color.Lerp(new Color(170, 230, 255), Color.White, tAvg) * (0.95f * easedFade);

                DrawSegmentLine(sb, pixel, start, end, outerWidth, outerColor);
                DrawSegmentLine(sb, pixel, start, end, coreWidth, coreColor);
                DrawSegmentLine(sb, pixel, start, end, innerWidth, innerColor);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 0.1f) return;
            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
