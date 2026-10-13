using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.particles;
namespace jarkos.projectiles.mecha
{
    public class Nucleon_wave_ring : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 500f;
        public const int LIFE = 20;
        private float Progress => 1f - Projectile.timeLeft / (float)LIFE;
        private float Radius => MaxRadius * EaseOut(Progress);
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LIFE;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            int size = (int)(Radius * 2);
            Projectile.position = Projectile.Center - new Vector2(size / 2);
            Projectile.width = size;
            Projectile.height = size;
            int particleCount = (int)(6 * (1f - Progress));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Radius;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                Color sparkColor = new Color(255, 100, 200);
                new Spark(pos, vel, sparkColor, 20, Main.rand.NextFloat(0.8f, 1.5f), 0.9f).Spawn();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            float r = Radius;
            float ringWidth = 40f;
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
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.4f, 0.8f));
                ringShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.1f);
                try
                {
                    Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", AssetRequestMode.ImmediateLoad).Value;
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
                spriteBatch.Draw(tex, drawPos, null, new Color(255, 100, 200) * alpha * 0.9f, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
