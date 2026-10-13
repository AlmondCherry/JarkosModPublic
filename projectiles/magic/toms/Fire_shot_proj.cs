using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.particles;
using Terraria.GameContent;
namespace jarkos.projectiles.magic.toms
{
	public class Fire_shot_proj : ModProjectile
	{
		private const int MaxTrail = 40;
        private List<Vector2> trail = new List<Vector2>();
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trail.Insert(0, Projectile.Center);
            if (trail.Count > MaxTrail)
                trail.RemoveAt(trail.Count - 1);
            Projectile.ai[0]++;
            if (Projectile.ai[0] < 150)
            {
                Projectile.velocity *= 0.94f;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            else if (Projectile.ai[0] < 180)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation += 0.05f;
            }
            else if (Projectile.ai[0] == 180)
            {
                Projectile.tileCollide = true;
                NPC target = FindNearest(1200f);
                if (target != null)
                {
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 24f;
                }
                else
                {
                    Projectile.velocity = new Vector2(24f, 0f).RotatedBy(Projectile.rotation);
                }
                for(int i = 0; i < 20; i++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f)).noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.5f }, Projectile.Center);
            }
            else
            {
                NPC target = FindNearest(1200f);
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 26f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.05f);
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
                float time = Projectile.ai[0] - 180;
                Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
                float waveSpeed = (float)Math.Cos(time * 0.15f) * 6f;
                Projectile.position += perp * waveSpeed;
            }
            float flicker = Main.rand.NextFloat(0.85f, 1.15f);
            float intensity = Projectile.ai[0] >= 150 ? 1.5f : 1.0f;
            Lighting.AddLight(Projectile.Center, 1.0f * intensity * flicker, 0.6f * intensity * flicker, 0.1f * intensity * flicker);
            SpawnCometDust();
        }
        private NPC FindNearest(float maxDist)
        {
            NPC closest = null;
            float minDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        private void SpawnCometDust()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8, 8);
                Vector2 dustVel = -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f)
                                  + Main.rand.NextVector2Circular(2, 2);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Torch, dustVel, 80, default, Main.rand.NextFloat(1.5f, 2.5f));
                fire.noGravity = true;
                fire.fadeIn = 1.4f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), DustID.SolarFlare, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(3, 3), 0, default, Main.rand.NextFloat(0.6f, 1.2f));
                Spark.noGravity = true;
            }
            float orbitAngle = Projectile.ai[0] * 0.25f;
            for (int i = 0; i < 2; i++)
            {
                float a = orbitAngle + i * MathHelper.Pi;
                float radius = 14f + (float)Math.Sin(Projectile.ai[0] * 0.1f) * 4f;
                Vector2 orbitPos = Projectile.Center + new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius);
                Dust orb = Dust.NewDustPerfect(orbitPos, DustID.FlameBurst, Vector2.Zero, 0, default, 0.9f);
                orb.noGravity = true;
                orb.noLight = false;
            }
            if (Projectile.ai[0] > 180 && Main.rand.NextBool(2))
            {
                new Spark(Projectile.Center + Main.rand.NextVector2Circular(8, 8), -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f), new Color(255, 200, 80), Main.rand.Next(25, 45), Main.rand.NextFloat(1.5f, 2.9f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawTrail(sb, pixel);
            DrawCometCore(sb, pixel);
            if (Projectile.ai[0] >= 150)
            {
                float starScale = 1f;
                if (Projectile.ai[0] < 180)
                {
                    starScale = (Projectile.ai[0] - 150) / 30f;
                }
                Vector2 pos = Projectile.Center - Main.screenPosition;
                sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 255, 255, 0) * starScale, 0f, new Vector2(0.5f, 0.5f), new Vector2(100f * starScale, 4f * starScale), SpriteEffects.None, 0f);
                sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 255, 255, 0) * starScale, MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(100f * starScale, 4f * starScale), SpriteEffects.None, 0f);
                sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 150, 50, 0) * starScale, 0f, new Vector2(0.5f, 0.5f), new Vector2(60f * starScale, 12f * starScale), SpriteEffects.None, 0f);
                sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 150, 50, 0) * starScale, MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(60f * starScale, 12f * starScale), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawTrail(SpriteBatch sb, Texture2D pixel)
        {
            if (trail.Count < 2)
                return;
            Color[] layerColors = new Color[]            {
                new Color(180, 30, 10), new Color(255, 100, 20), new Color(255, 200, 40), new Color(255, 255, 200), };
            float[] layerWidths = { 32f, 20f, 10f, 4f };
            float[] layerAlphas = { 0.2f, 0.35f, 0.55f, 0.85f };
            for (int layer = 0; layer < 4; layer++)
            {
                for (int i = 0; i < trail.Count - 1; i++)
                {
                    float progress = (float)i / (trail.Count - 1);
                    float fadeout = 1f - progress;
                    fadeout = fadeout * fadeout * fadeout;
                    float width = layerWidths[layer] * fadeout;
                    float alpha = layerAlphas[layer] * fadeout;
                    Vector2 start = trail[i] - Main.screenPosition;
                    Vector2 end = trail[i + 1] - Main.screenPosition;
                    Vector2 direction = end - start;
                    float length = direction.Length();
                    if (length < 0.5f)
                        continue;
                    float rotation = direction.ToRotation();
                    Color segColor = Color.Lerp(layerColors[layer], new Color(255, 255, 230), (1f - progress) * 0.2f) * alpha;
                    sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), segColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 1f, width), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawCometCore(SpriteBatch sb, Texture2D pixel)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float time = Projectile.ai[0];
            float speedFactor = MathHelper.Clamp(Projectile.velocity.Length() / 24f, 0.3f, 1f);
            float outerSize = 50f + (float)Math.Sin(time * 0.12f) * 10f;
            outerSize *= speedFactor;
            sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(200, 50, 10) * 0.25f, time * 0.03f, new Vector2(0.5f, 0.5f), new Vector2(outerSize, outerSize * 0.7f), SpriteEffects.None, 0f);
            float midSize = 30f + (float)Math.Sin(time * 0.18f) * 6f;
            midSize *= speedFactor;
            sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 130, 20) * 0.4f, -time * 0.05f, new Vector2(0.5f, 0.5f), new Vector2(midSize, midSize * 0.65f), SpriteEffects.None, 0f);
            float innerSize = 16f + (float)Math.Sin(time * 0.25f) * 3f;
            sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 230, 80) * 0.7f, time * 0.08f, new Vector2(0.5f, 0.5f), new Vector2(innerSize, innerSize), SpriteEffects.None, 0f);
            float hotSize = 8f + (float)Math.Sin(time * 0.3f) * 2f;
            sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), new Color(255, 255, 240) * 0.9f, 0f, new Vector2(0.5f, 0.5f), new Vector2(hotSize, hotSize), SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.OnFire3, 180);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f }, Projectile.Center);
            new Circleedge(Projectile.Center, Vector2.Zero, new Color(255, 100, 30), 40, 1.5f, 1f).Spawn();
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi / 15f * i;
                float speed = Main.rand.NextFloat(5f, 12f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                new Spark(Projectile.Center, vel, new Color(255, 200, 50), Main.rand.Next(30, 60), Main.rand.NextFloat(0.6f, 1.2f), 1f).Spawn();
            }
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi / 50f * i;
                float speed = Main.rand.NextFloat(3f, 10f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 50, default, Main.rand.NextFloat(1.8f, 3.0f));
                d.noGravity = true;
                d.fadeIn = 1.8f;
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8, 8);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15, 15), DustID.FlameBurst, vel, 0, default, Main.rand.NextFloat(1.2f, 2.2f));
                d.noGravity = true;
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12, 12);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, vel, 0, default, Main.rand.NextFloat(0.5f, 1.3f));
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 2f, 1.2f, 0.3f);
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
