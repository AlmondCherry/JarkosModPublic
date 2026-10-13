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
	public class SanguineRaindrop : ModProjectile
	{
        private static ReLogic.Content.Asset<Texture2D> healGlow;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
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
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.3f;
        }
        private float timer = 0f;
        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.velocity.Y < 20f)
                Projectile.velocity.Y += 0.08f;
            Lighting.AddLight(Projectile.Center, 0.6f, 0.05f, 0.1f);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, Main.rand.NextFloat(-0.3f, 0.3f), -1f, 80, default, 0.5f);
                d.noGravity = true;
                d.velocity.Y *= 0.3f;
            }
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), 0, 0, DustID.LifeDrain, 0, -0.5f, 100, new Color(180, 20, 20), 0.3f);
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = -0.4f, Volume = 0.6f }, target.Center);
            new Circular_glow(target.Center, Vector2.Zero, new Color(180, 20, 20), 12, 0.5f, 0.8f).Spawn();
            new Circleedge(target.Center, Vector2.Zero, new Color(255, 60, 60), 16, 0.3f, 0.7f).Spawn();
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(-MathHelper.Pi, 0);
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1f, 1.8f));
                d.noGravity = false;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY * -1) * Main.rand.NextFloat(5f, 12f);
                new Spark(target.Center, v, Main.rand.NextBool() ? new Color(150, 10, 10) : new Color(220, 30, 30), Main.rand.Next(15, 25), Main.rand.NextFloat(0.3f, 0.6f), 0.9f).Spawn();
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY * -1) * Main.rand.NextFloat(6f, 10f);
                new Stretching_particle(target.Center, v, new Color(180, 20, 20), 15, Main.rand.NextFloat(15f, 30f), Main.rand.NextFloat(2f, 4f), 0.05f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = -0.5f, Volume = 0.5f }, Projectile.Center);
            new Circular_glow(Projectile.Center, Vector2.Zero, new Color(160, 15, 15), 14, 0.4f, 0.8f).Spawn();
            for (int i = 0; i < 12; i++)
            {
                float angle = Main.rand.NextFloat(-MathHelper.Pi, 0);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = false;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY * -1) * Main.rand.NextFloat(3f, 8f);
                new Spark(Projectile.Center, v, new Color(180, 20, 20), Main.rand.Next(10, 20), Main.rand.NextFloat(0.3f, 0.5f), 0.85f).Spawn();
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
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            effect.Parameters["uOpacity"]?.SetValue(1f);
            effect.Parameters["uColor"]?.SetValue(new Color(120, 10, 10).ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(new Color(255, 60, 60).ToVector4());
            Vector2 half = Projectile.Size * 0.5f;
            Vector2[] points = new Vector2[Projectile.oldPos.Length + 1];
            points[0] = Projectile.Center;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
                points[i + 1] = Projectile.oldPos[i] == Vector2.Zero ? Vector2.Zero : Projectile.oldPos[i] + half;
            TrailRenderer.Draw(effect, points, t => 16f * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
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
                Main.spriteBatch.Draw(glow, pos, null, new Color(180, 20, 20) * (taper * 0.35f), 0f, glowOrigin, 0.7f * taper + 0.04f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, pos, null, new Color(255, 80, 80) * (taper * taper * 0.45f), 0f, glowOrigin, 0.35f * taper + 0.03f, SpriteEffects.None, 0f);
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
            float pulse = 1f + 0.15f * (float)Math.Sin(timer * 0.2f);
            if (glow != null)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(glow, drawPos, null, new Color(200, 20, 20) * 0.6f, 0f, glow.Size() * 0.5f, 0.7f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, drawPos, null, new Color(255, 100, 100) * 0.4f, 0f, glow.Size() * 0.5f, 0.4f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            float speedLength = Projectile.velocity.Length();
            float stretchY = 1f + MathHelper.Clamp(speedLength * 0.05f, 0f, 1.5f);
            float stretchX = 1f - MathHelper.Clamp(speedLength * 0.02f, 0f, 0.4f);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin, new Vector2(stretchX, stretchY), SpriteEffects.None, 0);
        }
	}
}
