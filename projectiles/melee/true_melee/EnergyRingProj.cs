using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class EnergyRingProj : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_0";
        private const int Lifetime = 26;
        private const float StartRadius = 14f;
        private const float EndRadius = 86f;
        private float Progress => 1f - Projectile.timeLeft / (float)Lifetime;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
        }
        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Lighting.AddLight(Projectile.Center, 0.9f * (1f - Progress), 0.7f * (1f - Progress), 0.2f * (1f - Progress));
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Effect effect = DevilShaders.EnergyRing;
            Texture2D noise = DevilShaders.Noise;
            if (effect == null || noise == null)
                return false;
            float progress = Progress;
            float radius = MathHelper.Lerp(StartRadius, EndRadius, MathSystem.EaseOutCubic(progress));
            effect.Parameters["uProgress"]?.SetValue(progress);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uColor"]?.SetValue(new Color(255, 200, 50).ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(new Color(255, 245, 200).ToVector4());
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
            float scale = radius * 2f / noise.Width;
            Main.spriteBatch.Draw(noise, Projectile.Center - Main.screenPosition, null, Color.White, 0f, noise.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
	}
}
