using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace Jarkos.projectiles.mecha
{
	public class Fulgur_shot_proj : ModProjectile
	{
        private Vector2[] oldPositions = new Vector2[20];
        private float[] oldRotations = new float[20];
        private List<Vector2> lightningPoints = new List<Vector2>();
        private NPC targetNPC = null;
        private int lightningTimer = 0;
        private bool hasStruckLightning = false;
        private float pulseTimer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            pulseTimer += 0.15f;
            Projectile.ai[0]++;


            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            for (int i = oldPositions.Length - 1; i > 0; i--)
            {
                oldPositions[i] = oldPositions[i - 1];
                oldRotations[i] = oldRotations[i - 1];
            }
            oldPositions[0] = Projectile.Center;
            oldRotations[0] = Projectile.rotation;

            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(20, 20);
                Dust dust = Dust.NewDustDirect(Projectile.Center + offset - new Vector2(4), 8, 8, DustID.Electric);
                dust.velocity = offset * 0.05f;
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }

            FindTarget();
            if (targetNPC != null && Projectile.ai[0] > 8)
            {
                StrikeLightning();
                Projectile.ai[0] = 0;
            }

            if (hasStruckLightning)
            {
                lightningTimer++;
                if (lightningTimer > 24)
                {
                    Projectile.Kill();
                }
            }
            Projectile.velocity *= 0.978f;
        }

        private void FindTarget()
        {
            float maxDistance = 350f;
            float closestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        closestDist = dist;
                        targetNPC = npc;
                    }
                }
            }
        }

        private void StrikeLightning()
        {
            if (targetNPC == null || !targetNPC.active) return;
            hasStruckLightning = true;
            GenerateLightningPath(Projectile.Center, targetNPC.Center);
            targetNPC.SimpleStrikeNPC(Projectile.damage, Projectile.direction > 0 ? 1 : -1, false, Projectile.knockBack);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);


            Vector2 pos = targetNPC.Center;
            new Circular_glow(pos, Vector2.Zero, new Color(40, 180, 255), 18, 0.6f, 0.95f).Spawn();

            int count = Main.rand.Next(2, 5);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + (i / (float)count) * MathHelper.TwoPi + Main.rand.NextFloat(-0.35f, 0.35f);
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(5.5f, 9.5f);
                float scale = Main.rand.NextFloat(0.22f, 0.38f);
                int lifetime = Main.rand.Next(18, 30);
                Color cyanColor = new Color(40, 180, 255);
                new Electric_particle(pos, particleVel, cyanColor, lifetime, scale, 1f, 0.92f).Spawn();
            }
        }

        private void GenerateLightningPath(Vector2 start, Vector2 end)
        {
            lightningPoints.Clear();
            lightningPoints.Add(start);
            Vector2 direction = end - start;
            Vector2 normalDir = Vector2.Normalize(direction);
            Vector2 perpendicular = new Vector2(-normalDir.Y, normalDir.X);
            int segments = 8;
            float jaggedness = 24f;

            for (int i = 1; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePoint = start + direction * progress;
                float offset = Main.rand.NextFloat(-jaggedness, jaggedness);
                float envelopeFactor = (float)Math.Sin(progress * Math.PI);
                offset *= envelopeFactor;
                Vector2 point = basePoint + perpendicular * offset;
                lightningPoints.Add(point);
            }
            lightningPoints.Add(end);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;


            float speed = Projectile.velocity.Length();
            float stretchX = 1f + speed * 0.04f;
            float squashY = Math.Max(0.45f, 1f - speed * 0.035f);
            Vector2 drawScale = new Vector2(stretchX, squashY) * Projectile.scale;

            for (int i = oldPositions.Length - 1; i >= 0; i--)
            {
                if (oldPositions[i] == Vector2.Zero) continue;
                float progress = 1f - (i / (float)oldPositions.Length);
                Vector2 trailScale = drawScale * progress * 0.8f;
                Color trailColor = new Color(100, 180, 255) * progress * 0.5f;
                spriteBatch.Draw(texture, oldPositions[i] - Main.screenPosition, null, trailColor, oldRotations[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.2f;
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Color glowColor = new Color(50, 150, 255) * 0.6f;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, 0f, glowTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);

            Color sphereColor = Color.White;
            Vector2 pulsedScale = drawScale * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3, 0).RotatedBy(i * MathHelper.PiOver2 + pulseTimer);
                spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, new Color(100, 200, 255, 0) * 0.5f, Projectile.rotation, origin, pulsedScale * 1.1f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, sphereColor, Projectile.rotation, origin, pulsedScale, SpriteEffects.None, 0f);

            if (lightningPoints.Count > 1)
            {
                DrawLightning(spriteBatch);
            }
            return false;
        }

        private void DrawLightning(SpriteBatch spriteBatch)
        {
            if (lightningPoints.Count < 2) return;

            float buildProgress = MathHelper.Clamp(lightningTimer / 5f, 0.1f, 1f);
            float fadeFactor = 1f - MathHelper.Clamp((lightningTimer - 5f) / (24f - 5f), 0f, 1f);
            float easedFade = fadeFactor * fadeFactor;

            if (easedFade <= 0.01f) return;

            int activeCount = (int)MathF.Ceiling(lightningPoints.Count * buildProgress);
            if (activeCount < 2) activeCount = 2;
            if (activeCount > lightningPoints.Count) activeCount = lightningPoints.Count;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 startDrawPos = lightningPoints[0] - Main.screenPosition;
            Vector2 endDrawPos = lightningPoints[activeCount - 1] - Main.screenPosition;

            spriteBatch.Draw(glowTex, startDrawPos, null, new Color(30, 130, 255) * (0.6f * easedFade), 0f, glowOrigin, 0.6f * easedFade, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, endDrawPos, null, new Color(230, 250, 255) * (0.75f * easedFade), 0f, glowOrigin, 0.45f * easedFade, SpriteEffects.None, 0f);

            for (int i = 0; i < activeCount - 1; i++)
            {
                Vector2 start = lightningPoints[i];
                Vector2 end = lightningPoints[i + 1];

                float t0 = (float)i / (lightningPoints.Count - 1);
                float t1 = (float)(i + 1) / (lightningPoints.Count - 1);
                float tAvg = (t0 + t1) * 0.5f;

                float outerWidth = MathHelper.Lerp(34f, 5f, tAvg) * easedFade;
                float coreWidth  = MathHelper.Lerp(7.5f, 1.8f, tAvg) * easedFade;
                float innerWidth = MathHelper.Lerp(3.5f, 0.9f, tAvg) * easedFade;

                Color outerColor = Color.Lerp(new Color(20, 100, 255), new Color(160, 210, 255), tAvg) * (0.35f * easedFade);
                Color coreColor  = Color.Lerp(new Color(50, 160, 255), Color.White, tAvg) * (0.85f * easedFade);
                Color innerColor = Color.Lerp(new Color(170, 230, 255), Color.White, tAvg) * (0.95f * easedFade);

                DrawLightningSegment(spriteBatch, pixel, start, end, outerWidth, outerColor);
                DrawLightningSegment(spriteBatch, pixel, start, end, coreWidth, coreColor);
                DrawLightningSegment(spriteBatch, pixel, start, end, innerWidth, innerColor);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void DrawLightningSegment(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 0.1f) return;
            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f }, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8, 8);
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(8), 16, 16, DustID.Electric);
                dust.velocity = vel;
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.0f, 2.0f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!hasStruckLightning)
            {
                FindTarget();
                if (targetNPC != null)
                {
                    StrikeLightning();
                    return false;
                }
            }
            return true;
        }
	}
}
