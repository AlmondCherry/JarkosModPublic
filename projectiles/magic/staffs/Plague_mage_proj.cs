using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;
using jarkos.projectiles.magic.toms;
namespace Jarkos.projectiles.magic.staffs
{
	public class Plague_mage_proj : ModProjectile
	{
        private static ReLogic.Content.Asset<Texture2D> healGlow;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 55;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            if (!Main.dedServ)
                healGlow = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/HealGlow");
        }
        public override void Unload()
        {
            healGlow = null;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0f && Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[0] = Projectile.velocity.X;
                Projectile.localAI[1] = Projectile.velocity.Y;
            }
            Vector2 baseVelocity = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            float t = (300 - Projectile.timeLeft) * 0.15f;
            float phase = Projectile.ai[0];
            float amplitude = 6.5f;
            Vector2 perp = new Vector2(-baseVelocity.Y, baseVelocity.X).SafeNormalize(Vector2.Zero);
            Projectile.velocity = baseVelocity + perp * amplitude * (float)Math.Cos(t + phase);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 0.4f * pulse, 0.8f * pulse, 0.1f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), DustID.Poisoned, -Projectile.velocity * 0.15f, 150, default, Main.rand.NextFloat(1.0f, 1.8f));
                trailDust.noGravity = true;
            }
            if (Main.rand.NextBool(10))
            {
                new Spark(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), new Color(150, 240, 50), Main.rand.Next(15, 25), Main.rand.NextFloat(0.3f, 0.5f), 0.85f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawHead();
            return false;
        }
        private void DrawTrail()
        {
            if (HealShaders.Stream != null)
                DrawShaderTrail();
            else
                DrawFallbackTrail();
        }
        private void DrawShaderTrail()
        {
            Effect effect = HealShaders.Stream;
            Main.spriteBatch.End();
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
            effect.Parameters["uOpacity"]?.SetValue(1f);
            effect.Parameters["uColor"]?.SetValue(new Color(90, 220, 20).ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(new Color(230, 255, 100).ToVector4());
            Vector2 half = Projectile.Size * 0.5f;
            Vector2[] points = new Vector2[Projectile.oldPos.Length + 1];
            points[0] = Projectile.Center;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
                points[i + 1] = Projectile.oldPos[i] == Vector2.Zero ? Vector2.Zero : Projectile.oldPos[i] + half;
            TrailRenderer.Draw(effect, points, t => 20f * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawFallbackTrail()
        {
            Texture2D glow = healGlow?.Value;
            if (glow == null)
                return;
            Vector2 half = Projectile.Size * 0.5f;
            Vector2 glowOrigin = glow.Size() * 0.5f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            int length = Projectile.oldPos.Length;
            int steps = length * 2 - 1;
            for (int s = steps - 1; s >= 0; s--)
            {
                int i = s / 2;
                Vector2 posWorld;
                if ((s & 1) == 0)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
                    posWorld = Projectile.oldPos[i];
                }
                else
                {
                    if (i + 1 >= length || Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                        continue;
                    posWorld = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], 0.5f);
                }
                float t = s / (float)steps;
                float taper = MathF.Pow(1f - t, 1.4f);
                Vector2 pos = posWorld + half - Main.screenPosition;
                Main.spriteBatch.Draw(glow, pos, null, new Color(130, 220, 20) * (taper * 0.3f), 0f, glowOrigin, 0.85f * taper + 0.04f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, pos, null, new Color(230, 255, 100) * (taper * taper * 0.4f), 0f, glowOrigin, 0.45f * taper + 0.03f, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawHead()
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glow = healGlow?.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f + Projectile.ai[0]);
            if (glow != null)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(glow, drawPos, null, new Color(110, 230, 30) * 0.65f, 0f, glow.Size() * 0.5f, 0.9f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, drawPos, null, new Color(220, 255, 120) * 0.5f, 0f, glow.Size() * 0.5f, 0.5f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin, pulse, SpriteEffects.None, 0);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.HasBuff(BuffID.Poisoned))
            {
                modifiers.FinalDamage *= 2f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
            new Circular_glow(target.Center, Vector2.Zero, new Color(130, 230, 20), 16, 0.45f, 0.8f).Spawn();
            new Circleedge(target.Center, Vector2.Zero, new Color(180, 255, 50), 18, 0.2f, 0.7f).Spawn();
            Vector2 dirOut = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 12; i++)
            {
                Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(4f, 10f);
                new Spark(target.Center, v, Main.rand.NextBool() ? new Color(110, 220, 20) : new Color(200, 255, 80), Main.rand.Next(15, 30), Main.rand.NextFloat(0.4f, 0.7f), 0.9f).Spawn();
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f)) * Main.rand.NextFloat(5f, 12f);
                new Stretching_particle(target.Center, v, new Color(150, 255, 40), 20, Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(3f, 5f), 0.05f).Spawn();
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Poisoned, velocity, 0, default, 1.5f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item107 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);
            new Circular_glow(Projectile.Center, Vector2.Zero, new Color(150, 255, 60), 20, 0.55f, 0.85f).Spawn();
            new Circleedge(Projectile.Center, Vector2.Zero, new Color(110, 200, 30), 24, 0.25f, 0.75f).Spawn();
            for (int i = 0; i < 20; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 12f);
                new Spark(Projectile.Center, v, Main.rand.NextBool() ? new Color(150, 255, 60) : new Color(200, 255, 100), Main.rand.Next(20, 40), Main.rand.NextFloat(0.5f, 0.8f), 0.85f).Spawn();
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(6f, 15f);
                new Stretching_particle(Projectile.Center, v, new Color(130, 240, 50), 25, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(4f, 7f), 0.04f).Spawn();
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(7f, 7f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.CursedTorch, velocity, 100, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 1; i++)
                {
                    Vector2 cloudVelocity = Main.rand.NextVector2Circular(3f, 3f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, cloudVelocity, ModContent.ProjectileType<PlagueCloudProjectile>(), Projectile.damage / 2, 0f, Projectile.owner);
                }
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
	}
	#region Cloud Projectile
	public class PlagueCloudProjectile : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/Empty_proj";
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.96f;
            Projectile.alpha += 1;
            if (Projectile.alpha > 250)
                Projectile.Kill();
            Projectile.position.X += (float)Math.Sin(Projectile.ai[0] * 0.05f) * 0.5f;
            Projectile.position.Y += (float)Math.Cos(Projectile.ai[0] * 0.04f) * 0.3f;
            Projectile.ai[0]++;
            float lightIntensity = (255 - Projectile.alpha) / 255f;
            Lighting.AddLight(Projectile.Center, 0.1f * lightIntensity, 0.5f * lightIntensity, 0.15f * lightIntensity);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30, 30), DustID.Poisoned, Main.rand.NextVector2Circular(1, 1), 200, default, Main.rand.NextFloat(0.5f, 1.2f));
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float alphaFactor = (255 - Projectile.alpha) / 255f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 6; i++)
            {
                float angle = Projectile.ai[0] * 0.02f + i * MathHelper.TwoPi / 6f;
                float dist = 10f + (float)Math.Sin(Projectile.ai[0] * 0.03f + i) * 5f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                float size = 25f + (float)Math.Sin(Projectile.ai[0] * 0.05f + i * 1.2f) * 8f;
                float localAlpha = alphaFactor * (0.15f + (float)Math.Sin(Projectile.ai[0] * 0.04f + i * 0.8f) * 0.05f);
                Color cloudColor = new Color(40, 200, 60) * localAlpha;
                spriteBatch.Draw(pixel, drawPos + offset, new Rectangle(0, 0, 1, 1), cloudColor, angle, new Vector2(0.5f, 0.5f), new Vector2(size, size), SpriteEffects.None, 0f);
            }
            float coreAlpha = alphaFactor * 0.25f;
            float coreSize = 35f + (float)Math.Sin(Projectile.ai[0] * 0.08f) * 5f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(80, 255, 100) * coreAlpha, Projectile.ai[0] * 0.01f, new Vector2(0.5f, 0.5f), new Vector2(coreSize, coreSize), SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 240);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * ((255 - Projectile.alpha) / 255f);
        }
    }
	#endregion
}
