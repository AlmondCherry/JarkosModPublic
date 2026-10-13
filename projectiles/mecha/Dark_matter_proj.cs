using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.systems;
namespace Jarkos.projectiles.mecha
{
    public class Dark_matter_proj : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        private float AccumulatedEnergy
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private int PiercedEnemies
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 70;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        public override void AI()
        {
            AccumulatedEnergy += 0.02f;
            AccumulatedEnergy = Math.Min(AccumulatedEnergy, 3f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            UpdateTrail();
            float pulse = 0.4f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
            Lighting.AddLight(Projectile.Center, 0.2f * pulse * AccumulatedEnergy, 0.8f * pulse * AccumulatedEnergy, 1f * pulse * AccumulatedEnergy);
            SpawnParticles();
            HomeTowardsEnemy();
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
            if (Main.rand.NextBool(12))
            {
                new Spark(Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), new Color(100, 220, 255), 14, Main.rand.NextFloat(0.4f, 0.7f), 0.8f).Spawn();
            }
        }
        private void HomeTowardsEnemy()
        {
            float homingRange = 150f;
            float homingStrength = 0.02f;
            NPC closestNPC = null;
            float closestDist = homingRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }
            if (closestNPC != null)
            {
                Vector2 direction = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), homingStrength);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            PiercedEnemies++;
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.7f, Pitch = 0.5f }, Projectile.Center);
            SpawnHitEffect(target.Center);
            if (Main.rand.NextBool(2) || PiercedEnemies >= Projectile.penetrate)
            {
                SpawnChainReaction(target);
            }
        }
        private void SpawnHitEffect(Vector2 position)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                new Spark(position, sparkVel, new Color(100, 200, 255), 18, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
            }
        }
        private void SpawnChainReaction(NPC target)
        {
            int fragmentCount = Main.rand.Next(1, 1);
            int fragmentDamage = (int)(Projectile.damage * (0.3f + AccumulatedEnergy * 0.15f));
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f, Pitch = 0.8f }, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.5f, Pitch = 1.2f }, Projectile.Center);
            float explosionScale = 1f + AccumulatedEnergy * 0.5f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f) * explosionScale;
                new Spark(Projectile.Center, sparkVel, new Color(100, 200, 255), 22, Main.rand.NextFloat(0.6f, 1.2f) * explosionScale, 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1)
            {
                if (HealShaders.Stream != null && HealShaders.HealNoise != null)
                {
                    Effect effect = HealShaders.Stream;
                    spriteBatch.End();
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(0, 130, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(200, 240, 255).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => (4f + AccumulatedEnergy * 2f) * MathF.Pow(1f - t, 0.35f), t => Color.White * (1f - t), HealShaders.HealNoise);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    DrawGlow(spriteBatch);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                else
                {
                    DrawNeutronTrail(spriteBatch, pixel);
                    DrawGlow(spriteBatch);
                }
            }
            return true;
        }
        private void DrawNeutronTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (trailPositions.Count < 2)
                return;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.8f;
                float thickness = (1f - progress) * (4f + AccumulatedEnergy * 2f);
                Color trailColor = Color.Lerp(new Color(150, 220, 255) * alpha, new Color(30, 80, 180) * alpha * 0.5f, progress);
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                if (length > 0)
                {
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * alpha * 0.5f, rotation, Vector2.Zero, new Vector2(length, thickness * 0.3f), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawGlow(SpriteBatch spriteBatch)
        {
            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.5f);
            float scale = (0.4f + AccumulatedEnergy * 0.15f) * pulse;
            Color glowColor = new Color(100, 200, 255) * 0.6f;
            spriteBatch.Draw(glowTexture, drawPos, null, glowColor, 0f, glowTexture.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #region Neutron Fragment
    public class NeutronFragment : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 12;
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.light = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Lighting.AddLight(Projectile.Center, 0.15f, 0.5f, 0.7f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0, 0, 150, default, 0.6f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f;
            }
            HomeToEnemy();
            Projectile.velocity *= 0.98f;
        }
        private void HomeToEnemy()
        {
            float homingRange = 120f;
            NPC target = null;
            float closestDist = homingRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
            }
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 8f, 0.08f);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.4f, Pitch = 1.5f }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.Electric, 0, 0, 100, new Color(100, 200, 255), 0.8f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(5, 5);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueTorch, 0, 0, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(4, 4);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.7f;
                float thickness = (1f - progress) * 3f;
                Color trailColor = new Color(80, 180, 255) * alpha;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                if (length > 0)
                {
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
                }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.6f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(100, 200, 255) * 0.4f, 0f, new Vector2(0.5f, 0.5f), new Vector2(12 * pulse, 12 * pulse), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), new Vector2(4 * pulse, 4 * pulse), SpriteEffects.None, 0f);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #endregion
}
