using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.projectiles.magic.scrolls;
using Jarkos.projectiles.bullet;
using jarkos.particles;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
namespace jarkos.projectiles.bullet
{
	public class Silent_arrow : ModProjectile
	{
        private const int MaxTrailPoints = 25;
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<float> trailRotations = new List<float>();
        private float distanceTraveled = 0f;
        private const float MaxBonusDamageDistance = 800f;
        private const float MaxDamageMultiplier = 1.8f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = MaxTrailPoints;
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
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
			Projectile.light = 0.75f;
        }
        public override void AI()
        {
            UpdateTrail();
            distanceTraveled += Projectile.velocity.Length();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float glowIntensity = Math.Min(distanceTraveled / MaxBonusDamageDistance, 1f);
            Lighting.AddLight(Projectile.Center, 0.4f * glowIntensity, 0.1f * glowIntensity, 0.6f * glowIntensity);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                dust.velocity = -Projectile.velocity * 0.1f;
                dust.noGravity = true;
                dust.scale = 0.6f + glowIntensity * 0.4f;
                dust.alpha = 100;
            }
            if (Projectile.ai[0] == 0f)
            {
                HomingBehavior();
            }
            if (Projectile.ai[0] == 1f)
            {
                Projectile.alpha += 1;
                if (Projectile.alpha > 200)
                    Projectile.Kill();
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            trailRotations.Insert(0, Projectile.rotation);
            if (trailPositions.Count > MaxTrailPoints)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailRotations.RemoveAt(trailRotations.Count - 1);
            }
        }
        private void HomingBehavior()
        {
            float homingRange = 200f;
            float homingStrength = 0.03f;
            NPC closestNPC = null;
            float closestDistance = homingRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }
            if (closestNPC != null)
            {
                Vector2 targetDirection = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDirection * Projectile.velocity.Length(), homingStrength);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float damageBonus = Math.Min(distanceTraveled / MaxBonusDamageDistance, 1f);
            float multiplier = 1f + (MaxDamageMultiplier - 1f) * damageBonus;
            modifiers.FinalDamage *= multiplier;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 180);
            target.AddBuff(BuffID.Slow, 120);
            if (Projectile.penetrate == 1)
            {
                SpawnShards(target.Center);
            }
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame);
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                dust.noGravity = true;
                dust.scale = 1.3f;
            }
        }
        private void SpawnShards(Vector2 position)
        {
            int shardCount = 4;
            float spreadAngle = MathHelper.TwoPi / shardCount;
            for (int i = 0; i < shardCount; i++)
            {
                float angle = spreadAngle * i + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = new Vector2(8f, 0f).RotatedBy(angle);
                Projectile.NewProjectile(Projectile.GetSource_Death(), position, velocity, ModContent.ProjectileType<SilentShardProjectile>(), Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            DrawTrail(pixel);
            return true;
        }
        private void DrawTrail(Texture2D pixel)
        {
            if (trailPositions.Count < 2)
                return;
            for (int i = 1; i < trailPositions.Count; i++)
            {
                Vector2 currentPos = trailPositions[i - 1];
                Vector2 previousPos = trailPositions[i];
                float progress = (float)i / trailPositions.Count;
                float width = MathHelper.Lerp(12f, 1f, progress);
                float alpha = MathHelper.Lerp(0.9f, 0f, progress);
                Color coreColor = Color.Lerp(new Color(180, 80, 255), new Color(40, 0, 80), progress) * alpha;
                Color edgeColor = Color.Lerp(new Color(100, 30, 180), new Color(20, 0, 40), progress) * (alpha * 0.6f);
                Vector2 direction = currentPos - previousPos;
                float segmentLength = direction.Length();
                if (segmentLength < 0.1f)
                    continue;
                float rotation = direction.ToRotation();
                Vector2 drawPos = previousPos - Main.screenPosition;
                DrawTrailSegment(pixel, drawPos, rotation, segmentLength, width * 1.8f, edgeColor);
                DrawTrailSegment(pixel, drawPos, rotation, segmentLength, width * 1.2f, edgeColor);
                DrawTrailSegment(pixel, drawPos, rotation, segmentLength, width * 0.6f, coreColor);
            }
        }
        private void DrawTrailSegment(Texture2D pixel, Vector2 position, float rotation, float length, float width, Color color)
        {
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            Main.EntitySpriteDraw(pixel, position, sourceRect, color, rotation, origin, scale, SpriteEffects.None, 0);
        }
        private void DrawGlow(Texture2D pixel)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.3f + 0.7f;
            float glowIntensity = Math.Min(distanceTraveled / MaxBonusDamageDistance, 1f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color glowColor = new Color(150, 50, 220) * (0.3f + glowIntensity * 0.4f) * pulse;
            for (int layer = 3; layer >= 0; layer--)
            {
                float size = 20f + layer * 12f;
                float layerAlpha = 1f / (layer + 1);
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 origin = new Vector2(0.5f, 0.5f);
                Main.EntitySpriteDraw(pixel, drawPos, sourceRect, glowColor * layerAlpha, 0f, origin, new Vector2(size, size), SpriteEffects.None, 0);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
	#region Shard
	public class SilentShardProjectile : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/bullet/silent_shard";
        private const int MaxTrailPoints = 15;
        private List<Vector2> trailPositions = new List<Vector2>();
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.alpha = 50;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
            Projectile.velocity.Y += 0.15f;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.05f, 0.3f);
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                dust.velocity = Vector2.Zero;
                dust.noGravity = true;
                dust.scale = 0.5f;
            }
            Projectile.alpha += 1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 90);
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.noGravity = true;
                dust.scale = 0.9f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 1; i < trailPositions.Count; i++)
            {
                Vector2 currentPos = trailPositions[i - 1];
                Vector2 previousPos = trailPositions[i];
                float progress = (float)i / trailPositions.Count;
                float width = MathHelper.Lerp(6f, 1f, progress);
                float alpha = MathHelper.Lerp(0.7f, 0f, progress);
                Color trailColor = new Color(130, 50, 180) * alpha * ((255 - Projectile.alpha) / 255f);
                Vector2 direction = currentPos - previousPos;
                float segmentLength = direction.Length();
                if (segmentLength < 0.1f)
                    continue;
                float rotation = direction.ToRotation();
                Vector2 drawPos = previousPos - Main.screenPosition;
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 origin = new Vector2(0f, 0.5f);
                Main.EntitySpriteDraw(pixel, drawPos, sourceRect, trailColor, rotation, origin, new Vector2(segmentLength, width), SpriteEffects.None, 0);
            }
            Vector2 shardPos = Projectile.Center - Main.screenPosition;
            float shardAlpha = (255 - Projectile.alpha) / 255f;
            for (int layer = 2; layer >= 0; layer--)
            {
                float size = 6f + layer * 4f;
                Color glowColor = new Color(160, 80, 220) * (0.5f / (layer + 1)) * shardAlpha;
                Main.EntitySpriteDraw(pixel, shardPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(size, size), SpriteEffects.None, 0);
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
	#endregion
}
