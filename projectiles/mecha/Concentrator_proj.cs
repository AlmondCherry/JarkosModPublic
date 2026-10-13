using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System.Collections.Generic;
using jarkos.particles;
using jarkos.systems;

namespace jarkos.projectiles.mecha
{
	public class Concentrator_proj : ModProjectile
	{
        private const int TrailLength = 20;
        private List<Vector2> trailPositions = new List<Vector2>();
        public float ConcentrationLevel => Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.light = 0f;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            float intensity = 0.5f + (ConcentrationLevel / 10f) * 0.5f;
            Lighting.AddLight(Projectile.Center, 0.2f * intensity, 0.5f * intensity, 1f * intensity);
            if (Main.rand.NextBool(3))
            {
                new Solid_particle(Projectile.Center, Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), new Color(80, 180, 255), 12 + (int)(ConcentrationLevel), 0.6f + (ConcentrationLevel / 10f) * 0.4f, 1f).Spawn();
            }
            float homingRange = 600f + ConcentrationLevel * 30f;
            float homingStrength = 0.08f + ConcentrationLevel * 0.01f;
            NPC target = FindClosestNPC(homingRange);
            if (target != null)
            {
                float speed = Projectile.velocity.Length();
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, homingStrength);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
            }
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
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawProjectileCore();
            return false;
        }
        private void DrawTrail()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(50, 150, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(200, 240, 255).ToVector4());
                float widthMultiplier = 1f + ConcentrationLevel * 0.1f;
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                points.AddRange(trailPositions);
                TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(12f * widthMultiplier, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawProjectileCore()
        {
            Texture2D glowTexture2 = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = new Vector2(glowTexture2.Width / 2, glowTexture2.Height / 2);
            float scale = 1f + ConcentrationLevel * 0.1f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f;
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTexture2, drawPos, null, new Color(50, 150, 255) * 0.5f, 0f, Origin, 0.4f * scale * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTexture2, drawPos, null, new Color(200, 240, 255), 0f, Origin, 0.2f * scale * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);
            CreateHitEffect(target.Center);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.4f }, Projectile.Center);
            CreateDeathEffect();
        }
        private void CreateHitEffect(Vector2 position)
        {
            float intensity = 1f + ConcentrationLevel * 0.15f;
            for (int i = 0; i < 3; i++)
            {
                Lighting.AddLight(position + Main.rand.NextVector2Circular(20f, 20f), 0.5f * intensity, 1f * intensity, 1.5f * intensity);
            }
            for (int i = 0; i < 2 + (int)(ConcentrationLevel * 2); i++)
            {
                new Spark(position, Main.rand.NextVector2Circular(8f, 8f) * intensity, new Color(100, 200, 255), 16 + (int)ConcentrationLevel, intensity, 1f).Spawn();
            }
            for (int i = 0; i < 3 + (int)ConcentrationLevel; i++)
            {
                new Spark(position, Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(1f, 3f), Color.Cyan, 10 + (int)ConcentrationLevel, 0.8f, 1f).Spawn();
            }
        }
        private void CreateDeathEffect()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 5; i++)
                {
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), new Color(80, 160, 255), 14, 0.7f, 1f).Spawn();
                }
            }
            Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 1f);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
}
