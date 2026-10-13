using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatoryDashPulseProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Misc/Perlin";
        private const int Lifetime = 26;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 255;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void AI()
        {
            float age = 1f - Projectile.timeLeft / (float)Lifetime;
            if (age < 0.85f && Main.rand.NextBool(2))
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + a.ToRotationVector2() * (40f + age * 120f);
                Vector2 vel = a.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4.5f);
                int dustId = Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.GoldFlame;
                Dust d = Dust.NewDustPerfect(pos, dustId, vel, Scale: Main.rand.NextFloat(1.0f, 1.7f));
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }
            Lighting.AddLight(Projectile.Center, 1.0f - age, 0.55f - age * 0.5f, 0.85f - age * 0.4f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float age = 1f - Projectile.timeLeft / (float)Lifetime;
            float radius = 18f + age * 220f;
            float thickness = MathHelper.Lerp(48f, 14f, age);
            float alpha = MathF.Pow(1f - age, 1.4f);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawCenter = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            const int segments = 28;
            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments * MathHelper.TwoPi;
                Vector2 dir = a.ToRotationVector2();
                Vector2 pos = drawCenter + dir * radius;
                float arcLen = MathHelper.TwoPi * radius / segments + 4f;
                Vector2 scale = new Vector2(arcLen, thickness);
                Color red = new Color(255, 90, 110, 0) * (alpha * 0.55f);
                Color cyan = new Color(120, 200, 255, 0) * (alpha * 0.45f);
                Color core = new Color(255, 220, 200, 0) * (alpha * 0.85f);
                float chrom = 6f * alpha;
                Main.spriteBatch.Draw(pixel, pos + dir * chrom, null, red, a, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, pos - dir * chrom, null, cyan, a, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, pos, null, core, a, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
            }
            float coreScale = MathHelper.Lerp(60f, 24f, age);
            Color flash = new Color(255, 200, 220, 0) * (alpha * 0.6f);
            Main.spriteBatch.Draw(pixel, drawCenter, null, flash, 0f, new Vector2(0.5f, 0.5f), new Vector2(coreScale * 2.4f, coreScale * 2.4f), SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
