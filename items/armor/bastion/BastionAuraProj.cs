using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using System;

namespace jarkos.items.armor.bastion
{
    public class BastionAuraProj : ModProjectile
    {
        public override string Texture => "jarkos/items/armor/bastion/FlameGlow";
        private Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = false;
        }
        public override void AI()
        {
            Player owner = Owner;
            var mp = owner.GetModPlayer<BastionPlayer>();
            if (!owner.active || owner.dead || (!mp.HasBastionSet && mp.AuraIntensity <= 0f))
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = owner.Center;
            Projectile.timeLeft = 6;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Owner;
            var mp = owner.GetModPlayer<BastionPlayer>();

            if (mp.AuraIntensity > 0.02f)
                DrawFlameAura(owner, mp);

            if (mp.HasBastionSet)
                DrawStackDigit(mp);

            return false;
        }
        private void DrawFlameAura(Player owner, BastionPlayer mp)
        {
            Vector2 drawPos = owner.Center - Main.screenPosition + new Vector2(0f, owner.gfxOffY);
            float burst = mp.BurstTimer;
            Vector2 quad = new Vector2(220f, 300f) * (1f + burst * 0.5f);

            if (BastionShaders.Flame != null)
            {
                Texture2D noise = ModContent.Request<Texture2D>("jarkos/items/armor/bastion/FireNoise").Value;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Effect effect = BastionShaders.Flame;

                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                effect.Parameters["uIntensity"]?.SetValue(mp.AuraIntensity);
                effect.Parameters["uSeed"]?.SetValue(Projectile.owner * 0.137f);
                effect.Parameters["uBurst"]?.SetValue(burst);
                effect.CurrentTechnique.Passes[0].Apply();

                Main.spriteBatch.Draw(noise, drawPos, null, Color.White, 0f, noise.Size() * 0.5f, new Vector2(quad.X / noise.Width, quad.Y / noise.Height), SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                Texture2D glow = TextureAssets.Projectile[Type].Value;
                Vector2 origin = glow.Size() * 0.5f;

                float t = Main.GlobalTimeWrappedHourly;
                float k = 1f + burst * 0.6f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < 6; i++)
                {
                    float ang = MathHelper.TwoPi * i / 6f + t * 1.3f;
                    Vector2 off = new Vector2((float)Math.Cos(ang) * 30f, (float)Math.Sin(ang) * 40f);

                    if (off.Y > 20f) off.Y = 20f;
                    Main.spriteBatch.Draw(glow, drawPos + off * k, null, new Color(255, 130, 30) * (0.30f * mp.AuraIntensity * (1f + burst)), 0f, origin, 1.6f * k, SpriteEffects.None, 0f);
                }

                for (int i = 0; i < 3; i++)
                {
                    float ph = t * (5.4f + i * 1.7f) + i * 2.1f;
                    Vector2 off = new Vector2((float)Math.Sin(ph) * 10f, -20f - i * (22f + burst * 16f));

                    Main.spriteBatch.Draw(glow, drawPos + off * k, null, new Color(255, 190, 70) * (0.4f * mp.AuraIntensity * (1f + burst)), 0f, origin, (1.9f - i * 0.4f) * k, SpriteEffects.None, 0f);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawStackDigit(BastionPlayer mp)
        {
            DynamicSpriteFont font = FontAssets.DeathText.Value;
            string text = mp.Stacks.ToString();

            Vector2 textOrigin = font.MeasureString(text) * 0.5f;
            Vector2 drawPos = mp.DigitPos - Main.screenPosition;

            float t = Main.GlobalTimeWrappedHourly;
            float scale = 0.62f * (1f + mp.DigitBump * 0.3f);
            float burn = mp.Stacks / 5f;
            float flash = mp.DigitFlash;

            Color hot = Color.Lerp(new Color(255, 130, 30), new Color(255, 235, 140), burn);
            Texture2D quadTex = TextureAssets.Projectile[Type].Value;
            Vector2 quadOrigin = quadTex.Size() * 0.5f;

            float quadScale = 96f / quadTex.Width;
            bool shader = BastionShaders.Digit != null;

            float ang = t * 2.8f;
            float depth = (float)Math.Sin(ang);

            Vector2 orbit = new Vector2((float)Math.Cos(ang) * 33f, depth * 11f - depth * 2f);
            Main.spriteBatch.End();

            if (shader)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                ApplyDigitShader(mp, -1f);

                Main.spriteBatch.Draw(quadTex, drawPos, null, Color.White, 0f, quadOrigin, quadScale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
            }
            else if (depth < 0f)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                DrawFallbackMote(quadTex, quadOrigin, drawPos + orbit, depth, burn);
                Main.spriteBatch.End();
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(i == 0 ? -2f : i == 1 ? 2f : 0f, i == 2 ? -2f : i == 3 ? 2f : 0f);
                Main.spriteBatch.DrawString(font, text, drawPos + off, new Color(40, 14, 6) * 0.9f, 0f, textOrigin, scale, SpriteEffects.None, 0f);
            }

            Color core = Color.Lerp(Color.Lerp(new Color(255, 244, 228), Color.White, burn), Color.White, flash);
            Main.spriteBatch.DrawString(font, text, drawPos, core, 0f, textOrigin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            float glowPower = 0.14f + burn * 0.6f + mp.DigitBump * 0.4f + flash * 0.9f;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < 3; i++)
            {
                float ph = t * (6.5f + i * 1.3f) + i * 2.09f;
                Vector2 off = new Vector2((float)Math.Sin(ph * 1.4f), -0.7f - 0.8f * (float)Math.Sin(ph * 0.9f)) * (1.1f + burn * 2.0f);

                Main.spriteBatch.DrawString(font, text, drawPos + off, hot * (glowPower * 0.4f), 0f, textOrigin, scale * (1.03f + i * 0.02f), SpriteEffects.None, 0f);
            }
            if (!shader && flash > 0.02f)
            {
                Main.spriteBatch.Draw(quadTex, drawPos, null, new Color(255, 240, 180) * flash, 0f, quadOrigin, 2.1f * flash + 0.7f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(quadTex, drawPos, null, new Color(255, 252, 230) * (flash * 0.8f), 0f, quadOrigin, 1.1f * flash + 0.4f, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            if (shader)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                ApplyDigitShader(mp, 1f);

                Main.spriteBatch.Draw(quadTex, drawPos, null, Color.White, 0f, quadOrigin, quadScale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
            }
            else if (depth >= 0f)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                DrawFallbackMote(quadTex, quadOrigin, drawPos + orbit, depth, burn);

                Main.spriteBatch.End();
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            if (mp.Stacks >= 3 && Main.rand.NextFloat() < 0.10f + burn * 0.15f)
            {
                Dust d = Dust.NewDustPerfect(mp.DigitPos + Main.rand.NextVector2Circular(9f, 11f), DustID.Torch, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.6f, -0.7f)), 0, default, Main.rand.NextFloat(0.7f, 1.2f));
                d.noGravity = true;
            }
        }
        private void ApplyDigitShader(BastionPlayer mp, float zSign)
        {
            Effect effect = BastionShaders.Digit;
            
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uStack"]?.SetValue(mp.Stacks / 5f);
            effect.Parameters["uFlash"]?.SetValue(mp.DigitFlash);
            effect.Parameters["uZSign"]?.SetValue(zSign);
            effect.CurrentTechnique.Passes[0].Apply();
        }
        private void DrawFallbackMote(Texture2D tex, Vector2 origin, Vector2 pos, float depth, float burn)
        {
            float s = 0.30f * (1f + depth * 0.3f);
            float br = (0.55f + burn * 0.5f) * (0.85f + depth * 0.25f);
            Main.spriteBatch.Draw(tex, pos, null, new Color(255, 220, 90) * br, 0f, origin, s, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, pos, null, new Color(255, 250, 200) * (br * 0.7f), 0f, origin, s * 0.45f, SpriteEffects.None, 0f);
        }
    }
}
