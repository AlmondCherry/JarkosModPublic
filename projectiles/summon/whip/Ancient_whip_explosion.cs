using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon.whip
{
	public class Ancient_whip_explosion : ModProjectile
	{
		private ref float ChargeProgress => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.localAI[0];
        private const int TOTAL_DURATION = 25;
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
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
            float scale = MathHelper.Clamp(ChargeProgress, 0.6f, 1.0f);
            int size = (int)(80 + 80 * scale);

            if (Projectile.width != size)
            {
                Vector2 center = Projectile.Center;
                Projectile.width = size;
                Projectile.height = size;
                Projectile.Center = center;
            }

            float fadeProgress = Timer / TOTAL_DURATION;
            int dustCount = (int)((1f - fadeProgress) * 3 * (1 + scale));

            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(2f, 4f) * scale;
                int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.Grass;
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(size * 0.2f, size * 0.2f), 1, 1, dustType);
                d.velocity = vel;
                d.scale = Main.rand.NextFloat(0.1f, 0.2f) * scale;
                d.noGravity = true;
                d.alpha = (int)(60 + fadeProgress * 150);
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.6f, 0.15f) * (1f - fadeProgress) * scale);
            Projectile.alpha = (int)(255 * fadeProgress);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 255) return false;
            if (MultiwaveShaderSystem.ExplosionShader == null)
                return false;

            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/Noises/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = noiseTex.Size() / 2f;

            float progress = 1f - Projectile.timeLeft / (float)TOTAL_DURATION;
            float scale = Projectile.width / (float)noiseTex.Width;

            MultiwaveShaderSystem.ExplosionShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
            MultiwaveShaderSystem.ExplosionShader.Parameters["uProgress"]?.SetValue(progress);
            MultiwaveShaderSystem.ExplosionShader.Parameters["uColor"]?.SetValue(new Vector3(0.4f, 1.0f, 0.3f));
            MultiwaveShaderSystem.ExplosionShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.17f);
            MultiwaveShaderSystem.ExplosionShader.CurrentTechnique.Passes[0].Apply();
            
            spriteBatch.Draw(noiseTex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
