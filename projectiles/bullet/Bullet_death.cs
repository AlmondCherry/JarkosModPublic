using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
namespace jarkos.projectiles.bullet
{
	public class Bullet_death : ModProjectile
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
            CreateAmbientParticles();
            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.8f);
            if (flightTime > MIN_FLIGHT_TIME && !hasTransformed)
            {
                NPC target = FindNearestEnemy();
                if (target != null)
                {
                    TransformToLightning(target);
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
                Dust Spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.Electric, Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 0, Color.Cyan, Main.rand.NextFloat(0.6f, 1f));
                Spark.noGravity = true;
                Spark.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust glow = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Vector2.Zero, 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                glow.noGravity = true;
            }
        }
        private NPC FindNearestEnemy()
        {
            NPC closest = null;
            float closestDist = DETECTION_RANGE;
            Vector2 direction = Vector2.Normalize(Projectile.velocity);
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist >= closestDist)
                    continue;
                Vector2 toEnemy = Vector2.Normalize(npc.Center - Projectile.Center);
                float dot = Vector2.Dot(direction, toEnemy);
                if (dot > -0.2f)
                {
                    if (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        closest = npc;
                        closestDist = dist;
                    }
                }
            }
            return closest;
        }
        private void TransformToLightning(NPC target)
        {
            hasTransformed = true;
            SoundEngine.PlaySound(SoundID.Item122 with
            {
                Volume = 0.8f, Pitch = 0.5f
            }, Projectile.Center);
            CreateDissolveEffect();
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FulguriteLightning>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: target.whoAmI, ai1: Projectile.Center.X, ai2: Projectile.Center.Y);
            }
            Projectile.Kill();
        }
        private void CreateDissolveEffect()
        {
            Vector2 pos = Projectile.Center;
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust electric = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10, 10), DustID.Electric, vel, 0, Color.Cyan, Main.rand.NextFloat(0.2f, 0.5f));
                electric.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust shard = Dust.NewDustPerfect(pos, DustID.Silver, vel, 0, default, Main.rand.NextFloat(0.2f, 0.5f));
                shard.noGravity = false;
            }
            for (int i = 0; i < 2; i++)
            {
                Dust flash = Dust.NewDustPerfect(pos, DustID.BlueTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(0.2f, 0.5f));
                flash.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item27 with
            {
                Volume = 0.6f, Pitch = 0.8f
            }, pos);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D arrowTexture = TextureAssets.Projectile[Projectile.type].Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawElectricTrail(sb, pixel);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawArrowWithGlow(sb, arrowTexture, lightColor);
            return false;
        }
        private void DrawElectricTrail(SpriteBatch sb, Texture2D pixel)
        {
            if (trailPositions.Count < 2) return;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.6f;
                float width = (1f - progress) * 4f;
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                DrawTrailSegment(sb, pixel, start, end, width, alpha);
            }
        }
        private void DrawTrailSegment(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, float alpha)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            float rotation = diff.ToRotation();
            Color color = new Color(100, 180, 255) * alpha;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            sb.Draw(pixel, start, sourceRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        private void DrawArrowWithGlow(SpriteBatch sb, Texture2D texture, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f;
            Color glowColor = new Color(100, 200, 255) * 0.3f * pulse;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2, 0).RotatedBy(i * MathHelper.PiOver2);
                sb.Draw(texture, drawPos + offset, null, glowColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            }
            sb.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
        }
        public override void OnKill(int timeLeft)
        {
            if (hasTransformed) return;
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(3f, 3f), 0, Color.Cyan, Main.rand.NextFloat(0.2f, 0.3f));
                dust.noGravity = true;
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
	#region Lighting
	public class FulguriteLightning_bullet : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/bullet/Bloody_bolt_impulse";
        private List<Vector2> mainBoltPoints = new List<Vector2>();
        private List<List<Vector2>> branchPoints = new List<List<Vector2>>();
        private const int BOLT_SEGMENTS = 12;
        private const float JAGGEDNESS = 35f;
        private const int MAX_BRANCHES = 4;
        private const float BRANCH_CHANCE = 0.3f;
        private bool initialized = false;
        private int animationTimer = 0;
        private const int ANIMATION_DURATION = 25;
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
            Projectile.timeLeft = ANIMATION_DURATION + 5;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = false;
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
            CreateLightningParticles();
            UpdateLighting();
            if (animationTimer % 4 == 0 && animationTimer < ANIMATION_DURATION - 5)
            {
                RegenerateBolt();
            }
            if (!hasDealtDamage && animationTimer == 3)
            {
                DealDamageToTarget();
                hasDealtDamage = true;
            }
            UpdateSounds();
        }
        private void InitializeLightning()
        {
            NPC target = Main.npc[TargetNPC];
            if (!target.active)
            {
                Projectile.Kill();
                return;
            }
            Vector2 start = StartPosition;
            Vector2 end = target.Center;
            mainBoltPoints = GenerateLightningBolt(start, end, BOLT_SEGMENTS, JAGGEDNESS);
            GenerateBranches();
            Projectile.Center = (start + end) / 2f;
            SoundEngine.PlaySound(SoundID.Item122 with
            {
                Volume = 1f, Pitch = -0.3f
            }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Thunder with
            {
                Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f)
            }, Projectile.Center);
        }
        private List<Vector2> GenerateLightningBolt(Vector2 start, Vector2 end, int segments, float jaggedness)
        {
            List<Vector2> points = new List<Vector2>();
            points.Add(start);
            Vector2 direction = end - start;
            float length = direction.Length();
            Vector2 normalDir = Vector2.Normalize(direction);
            Vector2 perpendicular = new Vector2(-normalDir.Y, normalDir.X);
            float segmentLength = length / segments;
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
        private void GenerateBranches()
        {
            branchPoints.Clear();
            if (mainBoltPoints.Count < 3) return;
            int branchCount = Main.rand.Next(2, MAX_BRANCHES + 1);
            for (int b = 0; b < branchCount; b++)
            {
                int branchIndex = Main.rand.Next(2, mainBoltPoints.Count - 2);
                Vector2 branchStart = mainBoltPoints[branchIndex];
                Vector2 mainDirection = Vector2.Normalize(mainBoltPoints[branchIndex + 1] - mainBoltPoints[branchIndex - 1]);
                Vector2 perpendicular = new Vector2(-mainDirection.Y, mainDirection.X);
                float branchAngle = Main.rand.NextFloat(-0.8f, 0.8f);
                Vector2 branchDir = mainDirection.RotatedBy(MathHelper.PiOver2 * (Main.rand.NextBool() ? 1 : -1) + branchAngle);
                float branchLength = Main.rand.NextFloat(40f, 120f);
                Vector2 branchEnd = branchStart + branchDir * branchLength;
                List<Vector2> branch = GenerateLightningBolt(branchStart, branchEnd, 5, JAGGEDNESS * 0.6f);
                branchPoints.Add(branch);
            }
        }
        private void RegenerateBolt()
        {
            if (mainBoltPoints.Count < 2) return;
            Vector2 start = mainBoltPoints[0];
            Vector2 end = mainBoltPoints[mainBoltPoints.Count - 1];
            mainBoltPoints = GenerateLightningBolt(start, end, BOLT_SEGMENTS, JAGGEDNESS * 0.7f);
            GenerateBranches();
        }
        private void CreateLightningParticles()
        {
            if (mainBoltPoints.Count < 2) return;
            float intensity = 1f - (float)animationTimer / ANIMATION_DURATION;
            if (Main.rand.NextBool(2))
            {
                int pointIndex = Main.rand.Next(mainBoltPoints.Count);
                Vector2 pos = mainBoltPoints[pointIndex];
                Dust electric = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10, 10), DustID.Electric, Main.rand.NextVector2Circular(3f, 3f), 0, Color.White, Main.rand.NextFloat(0.3f, 0.5f) * intensity);
                electric.noGravity = true;
            }
            if (Main.rand.NextBool(2) && mainBoltPoints.Count > 0)
            {
                Vector2 endPos = mainBoltPoints[mainBoltPoints.Count - 1];
                for (int i = 0; i < 2; i++)
                {
                    Dust Spark = Dust.NewDustPerfect(endPos + Main.rand.NextVector2Circular(15, 15), DustID.Electric, Main.rand.NextVector2Circular(5f, 5f), 0, Color.Cyan, Main.rand.NextFloat(0.2f, 0.5f) * intensity);
                    Spark.noGravity = true;
                }
            }
            foreach (var branch in branchPoints)
            {
                if (branch.Count > 0 && Main.rand.NextBool(4))
                {
                    Vector2 branchPos = branch[Main.rand.Next(branch.Count)];
                    Dust branchSpark = Dust.NewDustPerfect(branchPos, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, Color.Cyan, Main.rand.NextFloat(0.2f, 0.5f) * intensity);
                    branchSpark.noGravity = true;
                }
            }
        }
        private void UpdateLighting()
        {
            if (mainBoltPoints.Count < 2) return;
            float intensity = 1f - (float)animationTimer / ANIMATION_DURATION;
            float flicker = 0.7f + Main.rand.NextFloat(0.3f);
            for (int i = 0; i < mainBoltPoints.Count; i += 2)
            {
                Vector2 pos = mainBoltPoints[i];
                float lightIntensity = intensity * flicker;
                Lighting.AddLight(pos, 0.4f * lightIntensity, 0.6f * lightIntensity, 1f * lightIntensity);
            }
            if (mainBoltPoints.Count > 0)
            {
                Vector2 endPos = mainBoltPoints[mainBoltPoints.Count - 1];
                Lighting.AddLight(endPos, 0.6f * intensity, 0.8f * intensity, 1.2f * intensity);
            }
        }
        private void UpdateSounds()
        {
            if (animationTimer % 6 == 0 && animationTimer < ANIMATION_DURATION - 5)
            {
                SoundEngine.PlaySound(SoundID.Item93 with
                {
                    Volume = 0.4f * (1f - (float)animationTimer / ANIMATION_DURATION), Pitch = Main.rand.NextFloat(0.2f, 0.8f), MaxInstances = 3
                }, Projectile.Center);
            }
        }
        private void DealDamageToTarget()
        {
            NPC target = Main.npc[TargetNPC];
            if (!target.active) return;
            CreateHitEffect(target);
            SoundEngine.PlaySound(SoundID.Item94 with
            {
                Volume = 0.9f, Pitch = 0.3f
            }, target.Center);
        }
        private void CreateHitEffect(NPC target)
        {
            Vector2 pos = target.Center;
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust electric = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(target.width / 2, target.height / 2), DustID.Electric, vel, 0, Color.White, Main.rand.NextFloat(0.2f, 0.5f));
                electric.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Dust flash = Dust.NewDustPerfect(pos, DustID.BlueTorch, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(2f, 3f));
                flash.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Dust smoke = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(15, 15), DustID.Smoke, new Vector2(0, Main.rand.NextFloat(-2f, -0.5f)), 100, Color.LightGray, Main.rand.NextFloat(0.3f, 0.5f));
                smoke.noGravity = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (mainBoltPoints.Count < 2) return false;
            for (int i = 0; i < mainBoltPoints.Count - 1; i++)
            {
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), mainBoltPoints[i], mainBoltPoints[i + 1], 12f, ref point))
                {
                    return true;
                }
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 120);
            for (int i = 0; i < 3; i++)
            {
                Dust Spark = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width / 2, target.height / 2), DustID.Electric, Main.rand.NextVector2Circular(4f, 4f), 0, Color.Cyan, Main.rand.NextFloat(0.2f, 0.5f));
                Spark.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (mainBoltPoints.Count < 2) return false;
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float fadeProgress = 1f - (float)animationTimer / ANIMATION_DURATION;
            float flicker = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 1.5f) * 0.2f;
            float alpha = fadeProgress * flicker;
            foreach (var branch in branchPoints)
            {
                DrawLightningBolt(sb, pixel, branch, alpha * 0.6f, 0.5f);
            }
            DrawLightningBolt(sb, pixel, mainBoltPoints, alpha, 1f);
            DrawImpactGlow(sb, pixel, alpha);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawLightningBolt(SpriteBatch sb, Texture2D pixel, List<Vector2> points, float alpha, float widthScale)
        {
            if (points.Count < 2) return;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 start = points[i] - Main.screenPosition;
                Vector2 end = points[i + 1] - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Vector2 origin = new Vector2(0f, 0.5f);
                float outerWidth = 25f * widthScale;
                Color outerColor = new Color(30, 80, 180, 0) * alpha * 0.3f;
                sb.Draw(pixel, start, sourceRect, outerColor, rotation, origin, new Vector2(length, outerWidth), SpriteEffects.None, 0f);
                float midWidth = 14f * widthScale;
                Color midColor = new Color(80, 150, 255, 0) * alpha * 0.5f;
                sb.Draw(pixel, start, sourceRect, midColor, rotation, origin, new Vector2(length, midWidth), SpriteEffects.None, 0f);
                float coreWidth = 6f * widthScale;
                Color coreColor = new Color(150, 220, 255, 100) * alpha * 0.8f;
                sb.Draw(pixel, start, sourceRect, coreColor, rotation, origin, new Vector2(length, coreWidth), SpriteEffects.None, 0f);
                float innerWidth = 3f * widthScale;
                Color innerColor = Color.White * alpha * 0.9f;
                sb.Draw(pixel, start, sourceRect, innerColor, rotation, origin, new Vector2(length, innerWidth), SpriteEffects.None, 0f);
            }
            DrawBoltNodes(sb, pixel, points, alpha, widthScale);
        }
        private void DrawBoltNodes(SpriteBatch sb, Texture2D pixel, List<Vector2> points, float alpha, float widthScale)
        {
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            foreach (Vector2 point in points)
            {
                Vector2 drawPos = point - Main.screenPosition;
                float nodeSize = 8f * widthScale;
                Color nodeColor = new Color(150, 200, 255, 50) * alpha * 0.7f;
                sb.Draw(pixel, drawPos, sourceRect, nodeColor, 0f, new Vector2(0.5f), nodeSize, SpriteEffects.None, 0f);
                float coreSze = 3f * widthScale;
                Color coreColor = Color.White * alpha * 0.8f;
                sb.Draw(pixel, drawPos, sourceRect, coreColor, 0f, new Vector2(0.5f), coreSze, SpriteEffects.None, 0f);
            }
        }
        private void DrawImpactGlow(SpriteBatch sb, Texture2D pixel, float alpha)
        {
            if (mainBoltPoints.Count == 0) return;
            Vector2 impactPos = mainBoltPoints[mainBoltPoints.Count - 1] - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            float time = Main.GameUpdateCount * 0.2f;
            float pulse = 0.8f + (float)Math.Sin(time) * 0.2f;
            sb.Draw(pixel, impactPos, sourceRect, new Color(50, 100, 200, 0) * alpha * 0.3f * pulse, 0f, new Vector2(0.5f), 60f, SpriteEffects.None, 0f);
            sb.Draw(pixel, impactPos, sourceRect, new Color(100, 180, 255, 0) * alpha * 0.5f * pulse, 0f, new Vector2(0.5f), 35f, SpriteEffects.None, 0f);
            sb.Draw(pixel, impactPos, sourceRect, new Color(200, 230, 255, 100) * alpha * 0.8f * pulse, 0f, new Vector2(0.5f), 18f, SpriteEffects.None, 0f);
            sb.Draw(pixel, impactPos, sourceRect, Color.White * alpha * pulse, 0f, new Vector2(0.5f), 8f, SpriteEffects.None, 0f);
        }
        public override void OnKill(int timeLeft)
        {
            if (mainBoltPoints.Count > 0)
            {
                Vector2 endPos = mainBoltPoints[mainBoltPoints.Count - 1];
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(endPos + Main.rand.NextVector2Circular(20, 20), DustID.Electric, Main.rand.NextVector2Circular(4f, 4f), 0, Color.Cyan, Main.rand.NextFloat(0.2f, 0.3f));
                    dust.noGravity = true;
                }
            }
        }
	}
	#endregion
}
