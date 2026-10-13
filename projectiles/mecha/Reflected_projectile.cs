using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System.Collections.Generic;
namespace jarkos.projectiles.mecha
{
	public class Reflected_projectile : ModProjectile
	{
		private bool IsCritical => Projectile.ai[0] == 1;
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MAX_TRAIL = 20;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MAX_TRAIL)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (IsCritical)
            {
                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), 0.05f);
                }
            }
            SpawnTrailEffects();
            Color lightColor = IsCritical ? Color.Gold : Color.Cyan;
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * (IsCritical ? 1.5f : 1f));
        }
        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        private void SpawnTrailEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Color dustColor = IsCritical ? Color.Gold : Color.Cyan;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.FireworksRGB, -Projectile.velocity * 0.2f, 0, dustColor, IsCritical ? 1.8f : 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, sparkVel, 0, default, 1f);
                Spark.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnHitExplosion(target.Center);
            if (IsCritical && Main.rand.NextBool(2))
            {
                ChainLightning(target);
            }
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.5f }, target.Center);
        }
        private void ChainLightning(NPC origin)
        {
            float chainRange = 200f;
            int chainCount = 0;
            int maxChains = 3;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc == origin || chainCount >= maxChains) continue;
                float dist = Vector2.Distance(origin.Center, npc.Center);
                if (dist < chainRange)
                {
                    DrawLightningBetween(origin.Center, npc.Center);
                    int chainDamage = Projectile.damage / 2;
                    npc.SimpleStrikeNPC(chainDamage, Projectile.direction, false, Projectile.knockBack);
                    chainCount++;
                }
            }
        }
        private void DrawLightningBetween(Vector2 start, Vector2 end)
        {
            int segments = 10;
            Vector2 lastPos = start;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)(i + 1) / segments;
                Vector2 targetPos = Vector2.Lerp(start, end, t);
                if (i < segments - 1)
                {
                    Vector2 perpendicular = (end - start).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                    targetPos += perpendicular * Main.rand.NextFloat(-20f, 20f);
                }
                for (int j = 0; j < 5; j++)
                {
                    Vector2 dustPos = Vector2.Lerp(lastPos, targetPos, j / 5f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 0, default, 1.2f);
                    dust.noGravity = true;
                }
                lastPos = targetPos;
            }
        }
        private void SpawnHitExplosion(Vector2 position)
        {
            int dustCount = IsCritical ? 30 : 15;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                Color dustColor = IsCritical ? Color.Gold : Color.Cyan;
                Dust dust = Dust.NewDustPerfect(position, DustID.FireworksRGB, velocity, 0, dustColor, IsCritical ? 2f : 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawPlasmaTrail(spriteBatch);
            DrawProjectileCore(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawPlasmaTrail(SpriteBatch spriteBatch)
        {
            if (trailPositions.Count < 2) return;
            Texture2D trailTex = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Plasmamirror_proj").Value;
            for (int i = 1; i < trailPositions.Count; i++)
            {
                float progress = (float)i / trailPositions.Count;
                float width = (1f - progress) * (IsCritical ? 25f : 18f);
                float alpha = (1f - progress) * 0.8f;
                Vector2 start = trailPositions[i - 1];
                Vector2 end = trailPositions[i];
                Vector2 direction = end - start;
                float rotation = direction.ToRotation();
                float length = direction.Length();
                Color trailColor = IsCritical ?
                    Color.Lerp(Color.Gold, Color.OrangeRed, progress) :
                    Color.Lerp(Color.Cyan, Color.Blue, progress);
                trailColor *= alpha;
                spriteBatch.Draw(trailTex, start - Main.screenPosition, null, trailColor, rotation, new Vector2(0, trailTex.Height / 2f), new Vector2(length / trailTex.Width, width / trailTex.Height), SpriteEffects.None, 0f);
            }
        }
        private void DrawProjectileCore(SpriteBatch spriteBatch)
        {
            Texture2D coreTex = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Plasmamirror_proj").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f + 1f;
            Color glowColor = IsCritical ? Color.Gold : Color.Cyan;
            glowColor *= 0.6f;
            spriteBatch.Draw(coreTex, drawPos, null, glowColor, Projectile.rotation, coreTex.Size() / 2f, 1.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(coreTex, drawPos, null, Color.White * 0.9f, Projectile.rotation, coreTex.Size() / 2f, 0.8f * pulse, SpriteEffects.None, 0f);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, velocity, 0, default, 1.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }
	}
}
