using System;
using jarkos.buffs.buff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.other
{
    public class AbsoluteZeroProj : ModProjectile
    {
        private const int TrailLength = 25;
        private const int MaxPierces = 4;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailAlphas = new float[TrailLength];
        private bool trailInitialized = false;
        private float VisualIndex
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private float LiveTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private int PierceCount
        {
            get => (int)Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = MaxPierces;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.coldDamage = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            LiveTimer++;
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailAlphas[i] = 0f;
                }
                trailInitialized = true;
            }
            for (int i = TrailLength - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailAlphas[i] = trailAlphas[i - 1] * 0.96f;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (LiveTimer > 40)
            {
                Projectile.velocity.Y += 0.025f;
            }
            float wave = (float)Math.Sin(LiveTimer * 0.12f + VisualIndex) * 0.5f;
            Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
            Projectile.position += perp * wave;
            if (LiveTimer > 12)
            {
                NPC closest = FindClosestNPC(280f);
                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.025f);
                }
            }
            trailPositions[0] = Projectile.Center;
            trailAlphas[0] = 1f;
            SpawnFlightParticles();
            float pierceLight = 1f - PierceCount * 0.15f;
            Lighting.AddLight(Projectile.Center, 0.25f * pierceLight, 0.5f * pierceLight, 0.85f * pierceLight);
        }
        private void SpawnFlightParticles()
        {
            if (Main.rand.NextBool(3))
            {
                Dust ice = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), 0, 0, DustID.IceTorch, -Projectile.velocity.X * 0.08f + Main.rand.NextFloat(-0.4f, 0.4f), -Projectile.velocity.Y * 0.08f + Main.rand.NextFloat(-0.4f, 0.4f), 130, default, Main.rand.NextFloat(0.5f, 1f));
                ice.noGravity = true;
                ice.fadeIn = 0.7f;
            }
            if (Main.rand.NextBool(7))
            {
                Dust snow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Snow, -Projectile.velocity.X * 0.04f + Main.rand.NextFloat(-0.8f, 0.8f), -Projectile.velocity.Y * 0.04f + Main.rand.NextFloat(-0.8f, 0.8f), 180, Color.White, 0.7f);
                snow.noGravity = true;
            }
            if (Main.rand.NextBool(10))
            {
                Dust crystal = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueCrystalShard, Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-1.2f, 1.2f), 50, default, 0.6f);
                crystal.noGravity = true;
                crystal.fadeIn = 0.5f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            PierceCount++;
            Projectile.velocity *= 0.82f;
            target.AddBuff(BuffID.Frostburn2, 180);
            SpawnImpactParticles(target.Center);
            if (Main.rand.NextBool(2))
            {
                SoundEngine.PlaySound(SoundID.Item27 with
                {
                    Volume = 0.2f, Pitch = Main.rand.NextFloat(0.3f, 0.8f)
                }, target.Center);
            }
        }
        private void SpawnImpactParticles(Vector2 position)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                int dustType = Main.rand.Next(3) switch
                {
                    0 => DustID.IceTorch, 1 => DustID.Ice, _ => DustID.BlueCrystalShard
                };
                Dust d = Dust.NewDustDirect(position, 0, 0, dustType, vel.X, vel.Y, Main.rand.Next(60, 140), default, Main.rand.NextFloat(0.8f, 1.8f));
                d.noGravity = true;
                d.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Dust flash = Dust.NewDustDirect(position, 0, 0, DustID.Snow, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f), 200, Color.White, 1.5f);
                flash.noGravity = true;
            }
            Lighting.AddLight(position, 0.2f, 0.4f, 0.7f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27 with
            {
                Volume = 0.25f, Pitch = Main.rand.NextFloat(0.4f, 0.9f)
            }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4f);
                Dust shard = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Ice, vel.X, vel.Y, 100, default, Main.rand.NextFloat(0.8f, 1.5f));
                shard.noGravity = false;
                shard.fadeIn = 0.8f;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust cold = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.IceTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, default, 1.2f);
                cold.noGravity = true;
            }
        }
        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        #region MagicPixel Trail
        public override bool PreDraw(ref Color lightColor)
        {
            DrawSoftCyanGlow();
            DrawIceMagicPixelTrail();
            DrawIcicle(lightColor);
            return false;
        }

        private void DrawSoftCyanGlow()
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (glowTex == null) return;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 backOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 6f;
            Vector2 glowPos = drawPos + backOffset;

            float pulse = 1f + (float)Math.Sin(LiveTimer * 0.12f + VisualIndex) * 0.12f;
            Color cyanColor = new Color(80, 195, 255) * (0.55f * (1f - PierceCount * 0.15f));
            Vector2 glowOrigin = glowTex.Size() * 0.5f;
            Vector2 glowScale = new Vector2(0.65f, 0.65f) * pulse;

            sb.Draw(glowTex, glowPos, null, cyanColor, Projectile.rotation, glowOrigin, glowScale, SpriteEffects.None, 0f);


            Color coreCyan = new Color(180, 240, 255) * (0.35f * (1f - PierceCount * 0.15f));
            Vector2 coreScale = new Vector2(0.35f, 0.35f) * pulse;
            sb.Draw(glowTex, glowPos, null, coreCyan, Projectile.rotation, glowOrigin, coreScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawIceMagicPixelTrail()
        {
            trailPositions[0] = Projectile.Center;
            trailAlphas[0] = 1f;
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            for (int i = TrailLength - 1; i > 0; i--)
            {
                Vector2 current = trailPositions[i];
                Vector2 next = trailPositions[i - 1];
                if (current == Vector2.Zero || next == Vector2.Zero)
                    continue;
                Vector2 diff = next - current;
                float length = diff.Length();
                if (length < 0.5f) continue;
                float rotation = diff.ToRotation();
                float progress = 1f - (float)i / TrailLength;
                float segAlpha = trailAlphas[i];
                float baseWidth = MathHelper.Lerp(0.3f, 10f, progress);
                float crystalWave = (float)Math.Sin(LiveTimer * 0.12f + i * 0.6f + VisualIndex);
                float crystalEdge = Math.Abs(crystalWave) * 2.5f * progress;
                float width = Math.Max(baseWidth + crystalEdge, 0.3f);
                Color trailColor;
                float alpha;
                if (progress > 0.85f)
                {
                    trailColor = Color.Lerp(new Color(150, 220, 255), new Color(235, 248, 255), (progress - 0.85f) / 0.15f);
                    alpha = 0.9f;
                }
                else if (progress > 0.55f)
                {
                    float t = (progress - 0.55f) / 0.3f;
                    float shimmer = (float)Math.Sin(LiveTimer * 0.08f + i * 0.3f) * 0.5f + 0.5f;
                    Color mid = Color.Lerp(new Color(60, 160, 255), new Color(100, 200, 255), shimmer);
                    trailColor = Color.Lerp(new Color(30, 100, 200), mid, t);
                    alpha = MathHelper.Lerp(0.6f, 0.85f, t);
                }
                else if (progress > 0.2f)
                {
                    float t = (progress - 0.2f) / 0.35f;
                    trailColor = Color.Lerp(new Color(10, 40, 100), new Color(30, 100, 200), t);
                    alpha = MathHelper.Lerp(0.25f, 0.6f, t);
                }
                else
                {
                    trailColor = Color.Lerp(new Color(5, 15, 40, 0), new Color(10, 40, 100), progress / 0.2f);
                    alpha = MathHelper.Lerp(0f, 0.25f, progress / 0.2f);
                }
                alpha *= segAlpha;
                trailColor *= alpha;
                Vector2 origin = new Vector2(0, magicPixel.Height / 2f);
                Vector2 scale = new Vector2(length / magicPixel.Width, width / magicPixel.Height);
                Main.EntitySpriteDraw(magicPixel, current - Main.screenPosition, null, trailColor, rotation, origin, scale, SpriteEffects.None, 0);
                if (progress > 0.25f)
                {
                    float coreWidth = width * 0.3f;
                    Vector2 coreScale = new Vector2(length / magicPixel.Width, coreWidth / magicPixel.Height);
                    Color coreColor = Color.Lerp(new Color(180, 230, 255), new Color(240, 250, 255), progress) * (alpha * 0.5f);
                    Main.EntitySpriteDraw(magicPixel, current - Main.screenPosition, null, coreColor, rotation, origin, coreScale, SpriteEffects.None, 0);
                }
                if (progress > 0.15f && i % 3 == 0)
                {
                    Vector2 perp = new Vector2(-diff.Y, diff.X).SafeNormalize(Vector2.Zero);
                    float facetOffset = crystalWave * width * 0.6f;
                    float facetW = width * 0.2f;
                    float facetL = length * 0.7f;
                    Vector2 facetScale = new Vector2(facetL / magicPixel.Width, facetW / magicPixel.Height);
                    float facetAngle = rotation + crystalWave * 0.15f;
                    Color facetColor = new Color(120, 200, 255) * (alpha * 0.3f);
                    Vector2 facetPos = current + perp * facetOffset;
                    Main.EntitySpriteDraw(magicPixel, facetPos - Main.screenPosition, null, facetColor, facetAngle, origin, facetScale, SpriteEffects.None, 0);
                    Vector2 facetPos2 = current - perp * facetOffset;
                    Main.EntitySpriteDraw(magicPixel, facetPos2 - Main.screenPosition, null, facetColor, facetAngle, origin, facetScale, SpriteEffects.None, 0);
                }
                if (i % 4 == 0 && progress > 0.2f)
                {
                    float sparkle = (float)Math.Sin(LiveTimer * 0.4f + i * 2.1f + VisualIndex);
                    if (sparkle > 0.6f)
                    {
                        float sparkIntensity = (sparkle - 0.6f) / 0.4f;
                        float sparkSize = (2f + sparkIntensity * 4f) * progress;
                        Vector2 sparkCenter = new Vector2(magicPixel.Width / 2f, magicPixel.Height / 2f);
                        for (int ray = 0; ray < 4; ray++)
                        {
                            float rayAngle = MathHelper.PiOver4 * ray + LiveTimer * 0.03f;
                            float rayLength = sparkSize * 2f;
                            Vector2 rayScale = new Vector2(rayLength / magicPixel.Width, 1f / magicPixel.Height);
                            Color sparkColor = new Color(200, 240, 255) * (alpha * sparkIntensity * 0.4f);
                            Main.EntitySpriteDraw(magicPixel, current - Main.screenPosition, null, sparkColor, rayAngle, sparkCenter, rayScale, SpriteEffects.None, 0);
                        }
                        Vector2 dotScale = new Vector2(sparkSize / magicPixel.Width, sparkSize / magicPixel.Height);
                        Color dotColor = new Color(220, 245, 255) * (alpha * sparkIntensity * 0.5f);
                        Main.EntitySpriteDraw(magicPixel, current - Main.screenPosition, null, dotColor, 0f, sparkCenter, dotScale, SpriteEffects.None, 0);
                    }
                }
                if (progress > 0.1f && progress < 0.7f && i % 5 == 0)
                {
                    Vector2 perp3 = new Vector2(-diff.Y, diff.X).SafeNormalize(Vector2.Zero);
                    float hazeOffset = width * 1.2f + (float)Math.Sin(LiveTimer * 0.06f + i) * 4f;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 hazePos = current + perp3 * hazeOffset * side;
                        float hazeSize = 3f + progress * 2.5f;
                        Vector2 hazeScale = new Vector2(hazeSize / magicPixel.Width, hazeSize / magicPixel.Height);
                        Color hazeColor = new Color(80, 150, 220) * (alpha * 0.07f);
                        Vector2 hazOrigin = new Vector2(magicPixel.Width / 2f, magicPixel.Height / 2f);
                        Main.EntitySpriteDraw(magicPixel, hazePos - Main.screenPosition, null, hazeColor, 0f, hazOrigin, hazeScale, SpriteEffects.None, 0);
                    }
                }
            }
        }
        private void DrawIcicle(Color lightColor)
        {
            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor * 1.1f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            float addPulse = 0.22f + (float)Math.Sin(LiveTimer * 0.12f + VisualIndex) * 0.1f;
            Color additiveColor = new Color(100, 180, 255) * addPulse;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, additiveColor, Projectile.rotation, origin, Projectile.scale * 1.06f, SpriteEffects.None, 0);
            if (PierceCount > 0)
            {
                float pierceFade = Math.Min(PierceCount * 0.12f, 0.35f);
                Color frostColor = new Color(200, 230, 255) * pierceFade;
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, frostColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }
        }
    }
    #endregion
}
