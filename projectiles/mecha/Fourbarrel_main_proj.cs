using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class Fourbarrel_main_proj : ModProjectile
	{
        private List<Vector2> oldPositions = new List<Vector2>();
        private List<float> oldRotations = new List<float>();
        private const int TrailLength = 15;
        private int pulseTimer = 0;
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
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.light = 0.6f;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.position);
                Projectile.ai[0] = 1;
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            oldPositions.Insert(0, Projectile.Center);
            oldRotations.Insert(0, Projectile.rotation);
            if (oldPositions.Count > TrailLength)
            {
                oldPositions.RemoveAt(TrailLength);
                oldRotations.RemoveAt(TrailLength);
            }
            pulseTimer++;
            if (pulseTimer >= 8)
            {
                pulseTimer = 0;
                SpawnPulseTrail();
            }
            SpawnParticles();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 0.2f * pulse, 0.8f * pulse, 0.3f * pulse);
            if (Projectile.timeLeft < 170)
            {
                float wave = (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.whoAmI) * 0.3f;
                Projectile.velocity = Projectile.velocity.RotatedBy(wave * 0.02f);
            }
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(6))
            {
                new Circleedge_reducing(Projectile.Center, Projectile.velocity * -0.5f + Main.rand.NextVector2Circular(2f, 2f), Color.LimeGreen, 10, Main.rand.NextFloat(0.3f, 0.5f), 1f).Spawn();
            }
        }
        private void SpawnPulseTrail()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 spawnPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<ImpulseTrail>(), Projectile.damage / 3, 0f, Projectile.owner);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.position);
            if (Main.myPlayer == Projectile.owner && Main.rand.NextBool(3))
            {
                SpawnChainLightning(target);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    new Spark(Projectile.Center, vel, Color.LimeGreen, 16, Main.rand.NextFloat(0.8f, 1.5f), 1f).Spawn();
                }
            }
        }
        private void SpawnChainLightning(NPC hitTarget)
        {
            float maxDist = 200f;
            NPC closest = null;
            float closestDist = maxDist;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.whoAmI != hitTarget.whoAmI && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            if (closest != null)
            {
                Vector2 velocity = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<ImpulseChainBolt>(), Projectile.damage / 2, 0f, Projectile.owner, closest.whoAmI);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(50, 255, 100).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 255).ToVector4());
                if (oldPositions.Count > 1)
                {
                    List<Vector2> currentTrail = new List<Vector2>();
                    currentTrail.Add(Projectile.Center);
                    currentTrail.AddRange(oldPositions);
                    TrailRenderer.Draw(effect, currentTrail, t => MathHelper.Lerp(16f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            DrawProjectileCore(spriteBatch);
            return false;
        }
        private void DrawProjectileCore(SpriteBatch spriteBatch)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.2f + 0.8f;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(50, 255, 100) * pulse, 0f, glowTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * pulse, 0f, glowTex.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.4f, Pitch = 0.6f }, Projectile.position);
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GreenTorch, vel.X, vel.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GreenTorch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, Color.LimeGreen, 1f);
                Spark.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
    #region Trail
    public class ImpulseTrail : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private float maxScale = 1f;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 30f);
            if (lifeProgress < 0.3f)
            {
                maxScale = lifeProgress / 0.3f;
            }
            else if (lifeProgress > 0.7f)
            {
                maxScale = (1f - lifeProgress) / 0.3f;
            }
            else
            {
                maxScale = 1f;
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f + Projectile.whoAmI) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 0.1f * pulse * maxScale, 0.5f * pulse * maxScale, 0.15f * pulse * maxScale);
            if (Main.rand.NextBool(3) && maxScale > 0.5f)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 0, 0, DustID.GreenTorch, 0, 0, 100, default, 0.8f * maxScale);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (Projectile.timeLeft == 15)
            {
                SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.3f, Pitch = 0.8f }, Projectile.position);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GreenTorch, vel.X, vel.Y, 100, default, 1.2f);
                    dust.noGravity = true;
                }
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.timeLeft <= 18 && Projectile.timeLeft >= 12;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.5f + Projectile.whoAmI * 0.7f) * 0.2f + 0.8f;
            float scale = maxScale * pulse;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(50, 255, 150) * scale, 0f, glowTex.Size() / 2f, 0.4f * scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * scale, 0f, glowTex.Size() / 2f, 0.08f * scale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #endregion
    #region Chain
    public class ImpulseChainBolt : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> oldPositions = new List<Vector2>();
        private const int TrailLength = 10;
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            int targetIndex = (int)Projectile.ai[0];
            oldPositions.Insert(0, Projectile.Center);
            if (oldPositions.Count > TrailLength)
                oldPositions.RemoveAt(TrailLength);
            if (targetIndex >= 0 && targetIndex < Main.npc.Length)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active && !target.friendly)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), direction, 0.15f) * speed;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.1f, 0.4f, 0.2f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(2), 4, 4, DustID.GreenTorch, 0, 0, 0, Color.LimeGreen, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.4f, Pitch = 1f }, Projectile.position);
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.GreenTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, Color.LimeGreen, 0.8f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(50, 255, 100).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 255).ToVector4());
                if (oldPositions.Count > 1)
                {
                    TrailRenderer.Draw(effect, oldPositions, t => MathHelper.Lerp(8f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.6f) * 0.3f + 0.7f;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(50, 255, 100) * pulse, 0f, glowTex.Size() / 2f, 0.08f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * pulse, 0f, glowTex.Size() / 2f, 0.01f * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #endregion
}
