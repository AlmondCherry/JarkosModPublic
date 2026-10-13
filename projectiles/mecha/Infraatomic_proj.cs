using jarkos.particles;
using jarkos.projectiles.mecha;
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
	public class Infraatomic_proj : ModProjectile
	{
		private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 30;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 0.8f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.45f, 0.1f) * pulse);
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(15, 15);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Torch, -Projectile.velocity * 0.2f, 0, new Color(255, 120, 30), Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
            Projectile.ai[0]++;
            if (Projectile.ai[0] > 10)
            {
                float wave = (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.02f;
                Projectile.velocity = Projectile.velocity.RotatedBy(wave);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;


            if (trailPositions.Count > 0)
            {
                trailPositions[0] = Projectile.Center;
            }

            Texture2D sphereTex = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (trailPositions.Count > 1)
            {
                if (HealShaders.Stream != null && HealShaders.HealNoise != null)
                {
                    Effect effect = HealShaders.Stream;
                    spriteBatch.End();
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(255, 140, 20).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(255, 220, 140).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => 18f * MathF.Pow(1f - t, 0.4f), t => Color.White * (1f - t), HealShaders.HealNoise);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    DrawSoftGlow(spriteBatch, glowTex);
                    DrawProjectileCore(spriteBatch, sphereTex);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                else
                {
                    Texture2D pixel = TextureAssets.MagicPixel.Value;
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                        Vector2 direction = end - start;
                        float distance = direction.Length();
                        float rotation = direction.ToRotation();
                        float progress = (float)i / trailPositions.Count;
                        float width = MathHelper.Lerp(12f, 2f, progress);
                        float alpha = MathHelper.Lerp(1f, 0f, progress);
                        Color coreColor = Color.Lerp(new Color(255, 210, 100), new Color(255, 80, 0), progress) * alpha;
                        Color glowColor = new Color(255, 120, 20) * alpha * 0.6f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), glowColor, rotation, new Vector2(0, 0.5f), new Vector2(distance, width * 2.5f), SpriteEffects.None, 0f);
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(distance, width), SpriteEffects.None, 0f);
                    }
                    DrawSoftGlow(spriteBatch, glowTex);
                    DrawProjectileCore(spriteBatch, sphereTex);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return false;
        }
        private void DrawSoftGlow(SpriteBatch spriteBatch, Texture2D glowTex)
        {
            if (glowTex == null) return;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 0.85f;


            Color orangeSoftGlow = new Color(255, 130, 20) * 0.5f * pulse;
            spriteBatch.Draw(glowTex, center, null, orangeSoftGlow, 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);
        }
        private void DrawProjectileCore(SpriteBatch spriteBatch, Texture2D sphereTex)
        {
            if (sphereTex == null) return;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2 origin = sphereTex.Size() * 0.5f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 0.85f;


            spriteBatch.Draw(sphereTex, center, null, new Color(255, 90, 0) * 0.4f * pulse, 0f, origin, 1.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(sphereTex, center, null, new Color(255, 140, 20) * 0.95f * pulse, 0f, origin, 1.1f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(sphereTex, center, null, new Color(255, 210, 100) * pulse, 0f, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(sphereTex, center, null, Color.White * pulse, 0f, origin, 0.35f, SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnAtomicBurst(target.Center);
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 impactDir = Projectile.velocity.SafeNormalize(Vector2.Zero);
                if (impactDir == Vector2.Zero)
                {
                    impactDir = Main.rand.NextVector2CircularEdge(1f, 1f);
                }
                float maxDistance = 400f;
                var targets = new List<Tuple<NPC, float>>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.whoAmI != target.whoAmI && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && !npc.CountsAsACritter)
                    {
                        float dist = Vector2.Distance(target.Center, npc.Center);
                        if (dist <= maxDistance)
                        {
                            Vector2 toNPC = npc.Center - target.Center;
                            Vector2 toNPCDir = toNPC.SafeNormalize(Vector2.Zero);
                            float dot = Vector2.Dot(impactDir, toNPCDir);
                            if (dot >= 0.7071f)
                            {
                                if (Collision.CanHitLine(target.position, target.width, target.height, npc.position, npc.width, npc.height))
                                {
                                    targets.Add(new Tuple<NPC, float>(npc, dist));
                                }
                            }
                        }
                    }
                }
                targets.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                int linesSpawned = 0;
                foreach (var t in targets)
                {
                    if (linesSpawned >= 3) break;
                    int lineProj = Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<InfraatomicLine_proj>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack * 0.5f, Projectile.owner);
                    if (lineProj >= 0 && lineProj < Main.maxProjectiles)
                    {
                        Main.projectile[lineProj].ai[0] = target.Center.X;
                        Main.projectile[lineProj].ai[1] = target.Center.Y;
                        Main.projectile[lineProj].localAI[0] = t.Item1.whoAmI;
                        Main.projectile[lineProj].netUpdate = true;
                    }
                    linesSpawned++;
                }
            }
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(8, 8);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, velocity, ModContent.ProjectileType<AtomicParticle>(), Projectile.damage / 3, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }
        }
        private void SpawnAtomicBurst(Vector2 position)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8, 8);
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, velocity, 0, new Color(255, 150, 50), Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SpawnAtomicBurst(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f }, Projectile.Center);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #region Particle
    public class AtomicParticle : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 15;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 0.2f) * 0.5f);
            float homingRange = 150f;
            NPC target = null;
            float closestDist = homingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f);
            }
            Projectile.velocity *= 0.99f;
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.1f, 0, new Color(255, 150, 50), 0.8f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            if (trailPositions.Count > 1)
            {
                for (int i = 0; i < trailPositions.Count - 1; i++)
                {
                    Vector2 start = trailPositions[i] - Main.screenPosition;
                    Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                    Vector2 direction = end - start;
                    float distance = direction.Length();
                    float rotation = direction.ToRotation();
                    float progress = (float)i / trailPositions.Count;
                    float width = MathHelper.Lerp(6f, 1f, progress);
                    float alpha = MathHelper.Lerp(0.8f, 0f, progress);
                    Color trailColor = Color.Lerp(new Color(255, 180, 80), new Color(255, 80, 0), progress) * alpha;
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0, 0.5f), new Vector2(distance, width), SpriteEffects.None, 0f);
                }
            }
            Vector2 center = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.3f + 0.7f;
            spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 100, 0) * 0.5f * pulse, 0f, new Vector2(0.5f, 0.5f), 14f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 200, 100) * pulse, 0f, new Vector2(0.5f, 0.5f), 6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), 3f, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4, 4);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, velocity, 0, new Color(255, 150, 50), 1.2f);
                dust.noGravity = true;
            }
        }
    }
    #endregion
}
