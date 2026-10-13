using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using System.Collections.Generic;
using System;
using Terraria.Audio;
namespace jarkos.projectiles.melee.other_melee
{
	public class Energy_orb_spear : ModProjectile
    {
        public static List<Projectile> ActiveOrbs = new List<Projectile>();
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MAX_TRAIL = 20;
        private const float MAX_LIGHTNING_DISTANCE = 900f;
        private const int LIGHTNING_DAMAGE_COOLDOWN = 60;
        private int lightningDamageTimer = 0;
        private float glowPulse = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.6f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            if (!ActiveOrbs.Contains(Projectile))
                ActiveOrbs.Add(Projectile);
            glowPulse += 0.12f;
            Projectile.scale = 1f + (float)Math.Sin(glowPulse) * 0.15f;
            Projectile.rotation += 0.08f;
            Projectile.velocity *= 0.97f;
            if (Main.rand.NextBool(10))
            {
                Projectile.velocity += Main.rand.NextVector2Circular(0.3f, 0.3f);
            }
            UpdateTrail();
            lightningDamageTimer++;
            if (lightningDamageTimer >= LIGHTNING_DAMAGE_COOLDOWN)
            {
                DealLightningDamage();
                lightningDamageTimer = 0;
            }
            SpawnParticles();
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)MathHelper.Lerp(50, 255, 1f - Projectile.timeLeft / 30f);
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MAX_TRAIL)
                trailPositions.RemoveAt(trailPositions.Count - 1);
        }
        private void DealLightningDamage()
        {
            foreach (Projectile otherOrb in ActiveOrbs)
            {
                if (otherOrb.whoAmI == Projectile.whoAmI || !otherOrb.active) continue;
                float distance = Vector2.Distance(Projectile.Center, otherOrb.Center);
                if (distance > MAX_LIGHTNING_DISTANCE) continue;
                Vector2 start = Projectile.Center;
                Vector2 end = otherOrb.Center;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    if (DistanceToLineSegment(npc.Center, start, end) < npc.width + 20f)
                    {
                        int damage = Projectile.damage / 2;
                        Player player = Main.player[Projectile.owner];
                        npc.SimpleStrikeNPC(damage, 0, false, 0f, DamageClass.Melee);
                        npc.AddBuff(BuffID.Electrified, 180);
                        SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.8f, Volume = 0.3f }, npc.Center);
                        for (int i = 0; i < 8; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Electric, Main.rand.NextVector2Circular(4f, 4f));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                        }
                    }
                }
            }
        }
        private float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLengthSquared = line.LengthSquared();
            if (lineLengthSquared == 0f)
                return Vector2.Distance(point, lineStart);
            float t = Math.Max(0, Math.Min(1, Vector2.Dot(point - lineStart, line) / lineLengthSquared));
            Vector2 projection = lineStart + t * line;
            return Vector2.Distance(point, projection);
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.Electric, Main.rand.NextVector2Circular(2f, 2f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.6f, 1.1f);
            }
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, -Projectile.velocity * 0.5f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 240);
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.4f, Volume = 0.5f }, target.Center);
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Electric, Main.rand.NextVector2Circular(6f, 6f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.6f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            ActiveOrbs.Remove(Projectile);
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.6f, Volume = 0.4f }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(5f, 5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawLightningToOtherOrbs(spriteBatch);
            DrawTrail(spriteBatch);
            DrawGlow(spriteBatch);
            DrawMainSprite(spriteBatch, lightColor);
            return false;
        }
        private void DrawLightningToOtherOrbs(SpriteBatch spriteBatch)
        {
            foreach (Projectile otherOrb in ActiveOrbs)
            {
                if (otherOrb.whoAmI <= Projectile.whoAmI || !otherOrb.active) continue;
                if (otherOrb.type != Projectile.type) continue;
                float distance = Vector2.Distance(Projectile.Center, otherOrb.Center);
                if (distance > MAX_LIGHTNING_DISTANCE) continue;
                List<Vector2> lightningPoints = GenerateLightningPoints(Projectile.Center, otherOrb.Center);
                DrawLightning(spriteBatch, lightningPoints, distance);
            }
        }
        private List<Vector2> GenerateLightningPoints(Vector2 start, Vector2 end)
        {
            List<Vector2> points = new List<Vector2>();
            points.Add(start);
            Vector2 direction = end - start;
            float distance = direction.Length();
            int segments = Math.Max(4, (int)(distance / 20f));
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            perpendicular.Normalize();
            for (int i = 1; i < segments; i++)
            {
                float progress = i / (float)segments;
                Vector2 basePoint = Vector2.Lerp(start, end, progress);
                float offsetMultiplier = (float)Math.Sin(progress * MathHelper.Pi) * 0.15f;
                float maxOffset = distance * 0.15f * offsetMultiplier;
                float offset = Main.rand.NextFloat(-maxOffset, maxOffset);
                offset += (float)Math.Sin(Main.GameUpdateCount * 0.5f + i * 2f) * maxOffset * 0.3f;
                points.Add(basePoint + perpendicular * offset);
            }
            points.Add(end);
            return points;
        }
        private void DrawLightning(SpriteBatch spriteBatch, List<Vector2> points, float distance)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float intensity = 1f - (distance / MAX_LIGHTNING_DISTANCE) * 0.5f;
            float pulse = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f;
            intensity *= pulse;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 segmentStart = points[i];
                Vector2 segmentEnd = points[i + 1];
                Vector2 segmentDirection = segmentEnd - segmentStart;
                float segmentLength = segmentDirection.Length();
                float rotation = segmentDirection.ToRotation();
                DrawLightningSegment(spriteBatch, pixel, segmentStart, segmentLength, rotation, Color.Cyan * 0.15f * intensity, 12f);
                DrawLightningSegment(spriteBatch, pixel, segmentStart, segmentLength, rotation, Color.Cyan * 0.3f * intensity, 6f);
                DrawLightningSegment(spriteBatch, pixel, segmentStart, segmentLength, rotation, Color.White * 0.8f * intensity, 2f);
                DrawLightningSegment(spriteBatch, pixel, segmentStart, segmentLength, rotation, Color.White * intensity, 1f);
            }
        }
        private void DrawLightningSegment(SpriteBatch spriteBatch, Texture2D pixel, Vector2 position, float length, float rotation, Color color, float thickness)
        {
            spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
        }
        private void DrawTrail(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float progress = 1f - (i / (float)trailPositions.Count);
                Vector2 start = trailPositions[i];
                Vector2 end = trailPositions[i + 1];
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Color trailColor = Color.Lerp(Color.Transparent, Color.Cyan, progress) * progress * 0.6f;
                float thickness = 8f * progress;
                spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
            }
        }
        private void DrawGlow(SpriteBatch spriteBatch)
        {
            Texture2D glowTexture = TextureAssets.Extra[98].Value;
            Vector2 glowOrigin = glowTexture.Size() / 2f;
            float pulseScale = 0.6f + (float)Math.Sin(glowPulse) * 0.15f;
            spriteBatch.Draw(glowTexture, Projectile.Center - Main.screenPosition, null, Color.Cyan * 0.3f * (1f - Projectile.alpha / 255f), 0f, glowOrigin, pulseScale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTexture, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f * (1f - Projectile.alpha / 255f), 0f, glowOrigin, pulseScale * 0.5f, SpriteEffects.None, 0f);
        }
        private void DrawMainSprite(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = Color.Lerp(lightColor, Color.White, 0.5f);
            drawColor.A = (byte)(255 - Projectile.alpha);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }
    }
}
