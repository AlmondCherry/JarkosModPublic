using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class StormWave : ModProjectile
    {
        public const int LIFE = 42;
        private static Asset<Effect> waveShader;
        private static bool shaderLoadAttempted;
        private float MaxRadius => Projectile.ai[0];
        private float Intensity => Projectile.ai[1];
        private float Progress => 1f - Projectile.timeLeft / (float)LIFE;
        private float Radius => MaxRadius * EaseOut(Progress);
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = LIFE;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        public override void AI()
        {
            float r = Radius;
            int sparks = (int)(3 + 5 * Intensity);
            for (int i = 0; i < sparks; i++)
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + a.ToRotationVector2() * r;
                Dust d = Dust.NewDustPerfect(pos, DustID.Water, a.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f) - new Vector2(0f, 1f), 0, default, Main.rand.NextFloat(0.9f, 1.5f));
                d.noGravity = Main.rand.NextBool();
            }
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float r = Radius;
            Vector2 nearest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, nearest);
            return dist <= r + 10f && dist >= r - 34f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X >= Projectile.Center.X ? 1 : -1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                try
                {
                    waveShader = ModContent.Request<Effect>("jarkos/Shaders/StormWave", AssetRequestMode.ImmediateLoad);
                }
                catch { waveShader = null; }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float half = tex.Width / 2f;
            float scale = MaxRadius / (half * 0.82f);
            if (waveShader != null && waveShader.Value != null)
            {
                Effect fx = waveShader.Value;
                fx.Parameters["uProgress"]?.SetValue(Progress);
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uIntensity"]?.SetValue(0.5f + 0.5f * Intensity);
                fx.Parameters["uNoiseTexture"]?.SetValue(ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", AssetRequestMode.ImmediateLoad).Value);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, new Vector2(half, half), scale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float alpha = (1f - Progress) * (0.55f + 0.45f * Intensity);
                Main.spriteBatch.Draw(tex, drawPos, null, new Color(120, 190, 255, 0) * alpha, 0f, new Vector2(half, half), scale * EaseOut(Progress), SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
