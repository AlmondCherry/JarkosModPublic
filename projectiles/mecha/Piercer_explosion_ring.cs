using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
    public class Piercer_explosion_ring : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 500f;
        public const int LIFE = 12;
        private float Progress => 1f - Projectile.timeLeft / (float)LIFE;
        private float Radius => MaxRadius * EaseOut(Progress);
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.timeLeft = LIFE;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            int size = (int)(Radius * 2);
            Projectile.position = Projectile.Center - new Vector2(size / 2);
            Projectile.width = size;
            Projectile.height = size;
            Lighting.AddLight(Projectile.Center, 0.3f, 0.8f, 1f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            float r = Radius;
            float ringWidth = 60f;
            return distance <= r + ringWidth / 2 && distance >= r - ringWidth / 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float half = tex.Width / 2f;
            float scale = MaxRadius / (half * 0.82f);
            float currentScale = scale * EaseOut(Progress);
            Effect ringShader = null;
            try
            {
                ringShader = ModContent.Request<Effect>("jarkos/Shaders/SporeRing", AssetRequestMode.ImmediateLoad).Value;
            }
            catch { }
            if (ringShader != null)
            {
                ringShader.Parameters["uProgress"]?.SetValue(Progress);
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(0.2f, 0.8f, 1.0f));
                ringShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.1f);
                try
                {
                    Texture2D noise = ModContent.Request<Texture2D>("jarkos/textures/Circle_slashglow", AssetRequestMode.ImmediateLoad).Value;
                    ringShader.Parameters["uImage0"]?.SetValue(noise);
                }
                catch { }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, ringShader, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float alpha = 1f - Progress;
                spriteBatch.Draw(tex, drawPos, null, new Color(50, 200, 255) * alpha, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
