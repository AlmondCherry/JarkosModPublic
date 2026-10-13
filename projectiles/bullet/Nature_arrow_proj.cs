using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
using ReLogic.Content;
using jarkos.particles;
using jarkos.buffs.debuff;

namespace jarkos.projectiles.bullet
{
    public class Nature_arrow_proj : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int TRAIL_LENGTH = 8;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
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
            UpdateTrail();
            Projectile.velocity.Y += 0.08f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.Poisoned, Projectile.velocity * 0.1f, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.1f, 0.6f, 0.2f);
        }

        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            while (trailPositions.Count > TRAIL_LENGTH)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(ModContent.BuffType<Nature_weakness_debuff>(), 240);

            for (int i = 0; i < 15; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Poisoned, speed, 100, default, 1.4f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.7f, Pitch = 0.2f }, target.Center);


            if (Projectile.owner == Main.myPlayer)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal || npc.whoAmI == target.whoAmI)
                        continue;

                    float dist = Vector2.Distance(target.Center, npc.Center);
                    if (dist <= 1200f)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<NatureInfectionBeam>(), 0, 0f, Projectile.owner,
                        ai0: npc.whoAmI,
                        ai1: target.Center.X,
                        ai2: target.Center.Y
                        );
                    }
                }
            }
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
                float coreWidth = MathHelper.Lerp(4.5f, 0.5f, t0);

                Color glowColor = new Color(20, 200, 70) * ((1f - t0) * 0.45f);
                Color coreColor = Color.Lerp(Color.White, new Color(80, 255, 140), t0) * ((1f - t0) * 0.85f);

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
            Color greenGlow = new Color(30, 210, 80) * 0.55f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glowTex, drawPos, null, greenGlow, 0f, glowOrigin, 0.45f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(arrowTexture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Poisoned, Main.rand.NextVector2Circular(2.5f, 2.5f), 100, default, Main.rand.NextFloat(0.8f, 1.2f));
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

    public class NatureInfectionBeam : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/bullet/Bloody_bolt_impulse";
        private List<Vector2> beamPoints = new List<Vector2>();
        private const int BEAM_SEGMENTS = 8;
        private bool initialized = false;
        private int animationTimer = 0;
        private const int ANIMATION_DURATION = 22;
        private bool hasAppliedBuff = false;

        private int TargetNPC => (int)Projectile.ai[0];
        private Vector2 StartPosition => new Vector2(Projectile.ai[1], Projectile.ai[2]);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ANIMATION_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (!initialized)
            {
                InitializeBeam();
                initialized = true;
            }

            animationTimer++;

            if (!hasAppliedBuff && animationTimer >= 6)
            {
                ApplyPoisonToTarget();
                hasAppliedBuff = true;
            }
        }

        private void InitializeBeam()
        {
            NPC target = Main.npc[TargetNPC];
            Vector2 end = target.active ? target.Center : (StartPosition + Vector2.UnitY * 100f);
            Vector2 start = StartPosition;

            beamPoints = GenerateBeamPoints(start, end, BEAM_SEGMENTS);
            Projectile.Center = (start + end) / 2f;

            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.5f, Pitch = 0.05f }, Projectile.Center);
        }

        private List<Vector2> GenerateBeamPoints(Vector2 start, Vector2 end, int segments)
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
                float offset = Main.rand.NextFloat(-12f, 12f);
                float envelopeFactor = (float)Math.Sin(progress * Math.PI);
                offset *= envelopeFactor;

                Vector2 point = basePos + perpendicular * offset;
                points.Add(point);
            }
            points.Add(end);
            return points;
        }

        private void ApplyPoisonToTarget()
        {
            NPC target = Main.npc[TargetNPC];
            if (!target.active) return;

            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(ModContent.BuffType<Nature_weakness_debuff>(), 240);

            Vector2 pos = target.Center;
            new Circular_glow(pos, Vector2.Zero, new Color(40, 230, 90), 20, 0.65f, 0.95f).Spawn();

            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(12, 12), DustID.Poisoned, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (beamPoints.Count < 2) return false;


            float buildProgress = MathHelper.Clamp(animationTimer / 6f, 0.1f, 1f);
            float fadeFactor = 1f - MathHelper.Clamp((animationTimer - 6f) / (ANIMATION_DURATION - 6f), 0f, 1f);
            float easedFade = fadeFactor * fadeFactor;

            if (easedFade <= 0.01f) return false;

            int activeCount = (int)MathF.Ceiling(beamPoints.Count * buildProgress);
            if (activeCount < 2) activeCount = 2;
            if (activeCount > beamPoints.Count) activeCount = beamPoints.Count;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 startDrawPos = beamPoints[0] - Main.screenPosition;
            Vector2 endDrawPos = beamPoints[activeCount - 1] - Main.screenPosition;

            sb.Draw(glowTex, startDrawPos, null, new Color(30, 200, 80) * (0.6f * easedFade), 0f, glowOrigin, 0.6f * easedFade, SpriteEffects.None, 0f);
            sb.Draw(glowTex, endDrawPos, null, new Color(200, 255, 210) * (0.75f * easedFade), 0f, glowOrigin, 0.5f * easedFade, SpriteEffects.None, 0f);

            for (int i = 0; i < activeCount - 1; i++)
            {
                Vector2 start = beamPoints[i];
                Vector2 end = beamPoints[i + 1];

                float t0 = (float)i / (beamPoints.Count - 1);
                float t1 = (float)(i + 1) / (beamPoints.Count - 1);
                float tAvg = (t0 + t1) * 0.5f;

                float outerWidth = MathHelper.Lerp(32f, 5f, tAvg) * easedFade;
                float coreWidth  = MathHelper.Lerp(7.5f, 1.8f, tAvg) * easedFade;
                float innerWidth = MathHelper.Lerp(3.5f, 0.9f, tAvg) * easedFade;

                Color outerColor = Color.Lerp(new Color(20, 160, 60), new Color(140, 230, 170), tAvg) * (0.4f * easedFade);
                Color coreColor  = Color.Lerp(new Color(50, 220, 100), Color.White, tAvg) * (0.85f * easedFade);
                Color innerColor = Color.Lerp(new Color(180, 255, 200), Color.White, tAvg) * (0.95f * easedFade);

                DrawSegmentLine(sb, pixel, start, end, outerWidth, outerColor);
                DrawSegmentLine(sb, pixel, start, end, coreWidth, coreColor);
                DrawSegmentLine(sb, pixel, start, end, innerWidth, innerColor);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

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
