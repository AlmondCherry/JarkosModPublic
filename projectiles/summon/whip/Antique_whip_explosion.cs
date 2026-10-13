using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.summon.whip
{
	public class Antique_whip_explosion : ModProjectile
	{
        private ref float ScaleFactor => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.localAI[0];
        private float currentVisualRadius = 0f;
        private float maxVisualRadius;
        private float shockwaveRadius = 0f;
        private const int EXPAND_DURATION = 8;
        private const int HOLD_DURATION = 6;
        private const int FADE_DURATION = 16;
        private const int TOTAL_DURATION = EXPAND_DURATION + HOLD_DURATION + FADE_DURATION;
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TOTAL_DURATION;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            Timer++;
            float scale = MathHelper.Clamp(ScaleFactor, 0.8f, 2.5f);
            int baseSize = (int)(100 * scale);
            if (Projectile.width != baseSize)
            {
                Vector2 center = Projectile.Center;
                Projectile.width = baseSize;
                Projectile.height = baseSize;
                Projectile.Center = center;
            }
            maxVisualRadius = 60f * scale;
            if (Timer <= EXPAND_DURATION)
            {
                float expandProgress = Timer / EXPAND_DURATION;
                float eased = 1f - MathF.Pow(1f - expandProgress, 3f);
                currentVisualRadius = maxVisualRadius * eased;
                shockwaveRadius = maxVisualRadius * 2f * expandProgress;
            }
            else if (Timer <= EXPAND_DURATION + HOLD_DURATION)
            {
                float holdTime = Timer - EXPAND_DURATION;
                float pulse = 1f + MathF.Sin(holdTime * 0.8f) * 0.08f;
                currentVisualRadius = maxVisualRadius * pulse;
                shockwaveRadius = maxVisualRadius * 2f + holdTime * 3f;
            }
            else
            {
                float fadeProgress = (Timer - EXPAND_DURATION - HOLD_DURATION) / FADE_DURATION;
                float eased = 1f - fadeProgress;
                eased *= eased;
                currentVisualRadius = maxVisualRadius * eased * 0.7f;
                shockwaveRadius = maxVisualRadius * 2f + (HOLD_DURATION + (Timer - EXPAND_DURATION - HOLD_DURATION)) * 3f;
                Projectile.alpha = (int)(255 * fadeProgress);
            }
            SpawnExplosionDust(scale);
            float lightIntensity = MathHelper.Clamp(1f - (Timer / TOTAL_DURATION), 0f, 1f);
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.85f, 0.3f) * lightIntensity * scale);
        }
        private void SpawnExplosionDust(float scale)
        {
            if (scale >= 1.5f && Timer <= EXPAND_DURATION)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pillarPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-30, 30) * scale, 0);
                    Dust pillar = Dust.NewDustDirect(pillarPos, 1, 1, DustID.GoldFlame);
                    pillar.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-8f, -3f) * scale);
                    pillar.scale = Main.rand.NextFloat(1.5f, 2.5f);
                    pillar.noGravity = true;
                    pillar.alpha = 40;
                }
            }
            if (Timer > EXPAND_DURATION && Main.rand.NextBool(3))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(currentVisualRadius, currentVisualRadius), 1, 1, DustID.GoldCoin);
                Spark.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, -1f));
                Spark.scale = Main.rand.NextFloat(0.6f, 1.2f);
                Spark.noGravity = false;
                Spark.alpha = 80;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 255) return false;
            SpriteBatch sb = Main.spriteBatch;
            float opacity = 1f - Projectile.alpha / 255f;
            float scale = MathHelper.Clamp(ScaleFactor, 0.8f, 2.5f);
            DrawShockwave(sb, opacity, scale);
            return false;
        }
        private void DrawShockwave(SpriteBatch sb, float opacity, float scale)
        {
            if (shockwaveRadius <= 0) return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            int segments = 48;
            float ringThickness = MathHelper.Lerp(12f, 3f, Timer / TOTAL_DURATION) * scale;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle) * shockwaveRadius, MathF.Sin(angle) * shockwaveRadius);
                Vector2 nextPos = Projectile.Center + new Vector2(MathF.Cos(nextAngle) * shockwaveRadius, MathF.Sin(nextAngle) * shockwaveRadius);
                Vector2 diff = nextPos - pos;
                
                float segLen = diff.Length();
                float segRot = diff.ToRotation();
                float colorVar = MathF.Sin(angle * 3f + Timer * 0.3f) * 0.15f;
                Color ringColor = new Color(255, (int)(200 + colorVar * 55), (int)(60 + colorVar * 40));
                float ringAlpha = opacity * MathHelper.Clamp(1f - Timer / TOTAL_DURATION, 0f, 0.6f);
                sb.Draw(pixel, pos - Main.screenPosition, new Rectangle(0, 0, 1, 1), ringColor * ringAlpha, segRot, new Vector2(0.5f, 0.5f), new Vector2(segLen + 1f, ringThickness), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
