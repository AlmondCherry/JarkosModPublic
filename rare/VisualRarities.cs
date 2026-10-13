using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
namespace jarkos.rare
{
    public class CosmicRarity : ModRarity
    {
        public override Color RarityColor => new Color(180, 100, 255);
    }
    public class GalacticRarity : ModRarity
    {
        public override Color RarityColor => new Color(100, 255, 255);
    }
    public class SpeedRarity : ModRarity
    {
        public override Color RarityColor => new Color(255, 180, 50);
    }
    public class MagmaRarity : ModRarity
    {
        public override Color RarityColor => new Color(255, 60, 20);
    }
    public class GlitchRarity : ModRarity
    {
        public override Color RarityColor => new Color(0, 255, 100);
    }
    public class HolyRarity : ModRarity
    {
        public override Color RarityColor => new Color(255, 230, 100);
    }
    public class VoidRarity : ModRarity
    {
        public override Color RarityColor => new Color(150, 50, 255);
    }
    public class StormRarity : ModRarity
    {
        public override Color RarityColor => new Color(255, 50, 180);
    }
    public class RarityTooltipEffects : GlobalItem
    {
        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                if (item.rare == ModContent.RarityType<CosmicRarity>() || item.rare == ModContent.RarityType<GalacticRarity>())
                    DrawStarParticlesAndGlow(item, line);
                else if (item.rare == ModContent.RarityType<SpeedRarity>())
                    DrawSpeedLines(item, line);
                else if (item.rare == ModContent.RarityType<MagmaRarity>())
                    DrawMagmaEffect(item, line);
                else if (item.rare == ModContent.RarityType<GlitchRarity>())
                    DrawGlitchEffect(item, line);
                else if (item.rare == ModContent.RarityType<HolyRarity>())
                    DrawHolyEffect(item, line);
                else if (item.rare == ModContent.RarityType<VoidRarity>())
                    DrawVoidEffect(item, line);
                else if (item.rare == ModContent.RarityType<StormRarity>())
                    DrawStormEffect(item, line);
            }
            return true;
        }
        private void DrawStarParticlesAndGlow(Item item, DrawableTooltipLine line)
        {
            Texture2D starTex = ModContent.Request<Texture2D>("jarkos/textures/StarTexture").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);
            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);

            int particleCount = Math.Max(5, (int)(textSize.X / 15f));

            Color baseColor = item.rare == ModContent.RarityType<CosmicRarity>() ? new Color(255, 150, 255) : new Color(150, 255, 255);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float glowPulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.2f;
            float glowScale = textSize.X / glowTex.Width * 1.2f * glowPulse;

            Vector2 glowScaleVec = new Vector2(glowScale, glowScale * 0.5f);
            Main.spriteBatch.Draw(glowTex, textCenter, null, baseColor * 0.7f, 0f, glowTex.Size() / 2f, glowScaleVec, SpriteEffects.None, 0f);

            for (int i = 0; i < particleCount; i++)
            {
                float timeOffset = i * 2.14f;
                float time = Main.GlobalTimeWrappedHourly + timeOffset;

                float progress = time * 0.6f % 1f;
                float randomSeedX = i * 47f % textSize.X;
                float randomSeedY = i * 13f % (textSize.Y + 10f);
                float startX = line.X + randomSeedX + 20f;
                float startY = line.Y + randomSeedY + 10f;

                float moveDist = progress * 40f;
                float currentX = startX - moveDist;
                float currentY = startY - moveDist;

                float alpha = (float)Math.Sin(progress * MathHelper.Pi);
                float scale = 0.008f + (alpha * 0.1f) + ((i % 3) * Main.rand.NextFloat(0.002f, 0.015f));

                Main.spriteBatch.Draw(starTex, new Vector2(currentX, currentY), null, baseColor * alpha, progress * MathHelper.TwoPi, starTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private void DrawSpeedLines(Item item, DrawableTooltipLine line)
        {
            Texture2D flareTex = ModContent.Request<Texture2D>("jarkos/textures/Flare").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);
            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);

            int lineCount = 12;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float glowPulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.2f;
            float glowScale = textSize.X / glowTex.Width * 1.2f * glowPulse;

            Vector2 glowScaleVec = new Vector2(glowScale, glowScale * 0.5f);
            Color glowColor = new Color(255, 180, 50) * 0.7f;

            Main.spriteBatch.Draw(glowTex, textCenter, null, glowColor, 0f, glowTex.Size() / 2f, glowScaleVec, SpriteEffects.None, 0f);
            for (int i = 0; i < lineCount; i++)
            {
                float speedMultiplier = 3f + i % 4 * 1f;
                float timeOffset = i * 4.17f;
                float time = (Main.GlobalTimeWrappedHourly * speedMultiplier) + timeOffset;

                float progress = time % 1f;
                float travelDistance = textSize.X + 100f;

                float startX = line.X - 50f;
                float currentX = startX + (progress * travelDistance);
                float randomSeedY = i * 29f % (textSize.Y + 6f);
                float currentY = line.Y - 3f + randomSeedY;

                float alpha = 4f * progress * (1f - progress);
                float scaleX = 1.5f + (progress * 1.5f);
                float scaleY = 0.1f + (i % 3 * 0.05f);
                Main.spriteBatch.Draw(flareTex, new Vector2(currentX, currentY), null, new Color(255, 220, 100) * alpha, 0f, flareTex.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private void DrawMagmaEffect(Item item, DrawableTooltipLine line)
        {
            Texture2D dustTex = ModContent.Request<Texture2D>("jarkos/particles/Dust1_particle").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);
            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float glowPulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.5f) * 0.2f;
            float glowScale = textSize.X / glowTex.Width * 1.3f * glowPulse;
            Color glowColor = new Color(255, 60, 20) * 0.6f;

            Main.spriteBatch.Draw(glowTex, textCenter, null, glowColor, 0f, glowTex.Size() / 2f, new Vector2(glowScale, glowScale * 0.6f), SpriteEffects.None, 0f);
            int particleCount = Math.Max(6, (int)(textSize.X / 12f));

            for (int i = 0; i < particleCount; i++)
            {
                float timeOffset = i * 5.12f;
                float time = Main.GlobalTimeWrappedHourly * 0.8f + timeOffset;
                float progress = time % 1f;

                float randomSeedX = i * 37f % textSize.X;
                float currentX = line.X + randomSeedX + (float)Math.Sin(progress * MathHelper.TwoPi + i) * 5f;
                float startY = line.Y + textSize.Y + 5f;
                float travelDistance = textSize.Y + 15f;
                float currentY = startY - (progress * travelDistance);

                float alpha = (float)Math.Sin(progress * MathHelper.Pi);
                float scale = 0.4f + (alpha * 0.3f) + (i % 3 * 0.1f);
                Color drawColor = new Color(255, 100, 30) * alpha;
                Main.spriteBatch.Draw(dustTex, new Vector2(currentX, currentY), null, drawColor, progress * MathHelper.TwoPi, dustTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private void DrawGlitchEffect(Item item, DrawableTooltipLine line)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            int glitchCount = 8;

            for (int i = 0; i < glitchCount; i++)
            {
                float timeOffset = i * 0.77f;
                float snappedTime = (float)Math.Floor(Main.GlobalTimeWrappedHourly * 15f + timeOffset);
                float seed = snappedTime * (i + 1);

                float randomX = seed * 17f % textSize.X;
                float randomY = seed * 23f % textSize.Y;

                float width = 2f + seed * 7f % 15f;
                float height = 1f + seed * 11f % 4f;
                float currentX = line.X + randomX;
                float currentY = line.Y + randomY;

                float alpha = (seed * 13f % 100f) > 50f ? 0.8f : 0.2f;
                Color drawColor = new Color(0, 255, 100) * alpha;
                Main.spriteBatch.Draw(pixel, new Rectangle((int)currentX, (int)currentY, (int)width, (int)height), drawColor);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private void DrawHolyEffect(Item item, DrawableTooltipLine line)
        {
            Texture2D lineTex = ModContent.Request<Texture2D>("jarkos/particles/Line_glow").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Texture2D starTex = ModContent.Request<Texture2D>("jarkos/textures/StarTexture").Value;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);
            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float glowPulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.5f) * 0.2f;
            float glowScale = textSize.X / glowTex.Width * 1.2f * glowPulse;
            Color glowColor = new Color(255, 230, 100) * 0.4f;

            Main.spriteBatch.Draw(glowTex, textCenter, null, glowColor, 0f, glowTex.Size() / 2f, new Vector2(glowScale, glowScale * 0.5f), SpriteEffects.None, 0f);

            float starDuration = 2.2f;
            float starTime = Main.GlobalTimeWrappedHourly % starDuration;

            float starProgress = starTime / starDuration;

            float starEase = 1f - (float)Math.Pow(1f - starProgress, 3f);
            float starStartX = line.X - 25f;
            float starEndX = line.X + textSize.X + 25f;

            float starX = MathHelper.Lerp(starStartX, starEndX, starEase);
            float starY = line.Y + textSize.Y / 2f;

            Vector2 starPos = new Vector2(starX, starY);
            float starAlpha = 1f - starProgress;
            if (starProgress < 0.15f)
            {
                starAlpha *= starProgress / 0.15f;
            }
            starAlpha *= 0.6f;
            float starScale = 0.07f * (1f - starProgress * 0.2f);
            float starRotation = starProgress * 1.5f;
            for (int j = 1; j <= 4; j++)
            {
                float trailProgress = starProgress - (j * 0.022f);
                if (trailProgress < 0f) continue;

                float trailEase = 1f - (float)Math.Pow(1f - trailProgress, 3f);
                float trailX = MathHelper.Lerp(starStartX, starEndX, trailEase);

                Vector2 trailPos = new Vector2(trailX, starY);

                float trailAlpha = 1f - trailProgress;
                if (trailProgress < 0.15f)
                {
                    trailAlpha *= trailProgress / 0.15f;
                }
                trailAlpha *= 0.6f * (1f - (j / 5f)) * 0.4f;

                float trailScale = 0.07f * (1f - trailProgress * 0.2f) * (1f - j / 5f * 0.4f);
                Color trailColor = new Color(255, 235, 140) * trailAlpha;
                float trailRotation = trailProgress * 1.5f - (j * 0.04f);

                Main.spriteBatch.Draw(starTex, trailPos, null, trailColor, trailRotation, starTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }
            float starGlowScale = 0.07f * 3.5f * (1f - starProgress * 0.2f);
            Color starGlowColor = new Color(255, 220, 100) * starAlpha * 0.35f;

            Main.spriteBatch.Draw(glowTex, starPos, null, starGlowColor, 0f, glowTex.Size() / 2f, starGlowScale, SpriteEffects.None, 0f);

            float crossLength = 28f * (1f - starProgress * 0.2f);
            float crossScaleX = crossLength / lineTex.Width;
            float crossScaleY = 0.06f;

            Color crossColor = new Color(255, 245, 180) * starAlpha * 0.45f;
            Main.spriteBatch.Draw(lineTex, starPos, null, crossColor, starRotation, lineTex.Size() / 2f, new Vector2(crossScaleX, crossScaleY), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(lineTex, starPos, null, crossColor, starRotation + MathHelper.PiOver2, lineTex.Size() / 2f, new Vector2(crossScaleX * 0.6f, crossScaleY), SpriteEffects.None, 0f);

            Color starColor = new Color(255, 245, 180) * starAlpha;
            Main.spriteBatch.Draw(starTex, starPos, null, starColor, starRotation, starTex.Size() / 2f, starScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private void DrawVoidEffect(Item item, DrawableTooltipLine line)
        {
            Texture2D cubeTex = ModContent.Request<Texture2D>("jarkos/particles/Cube_edge_particle").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);
            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float glowPulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1f) * 0.2f;
            float glowScale = textSize.X / glowTex.Width * 1.2f * glowPulse;

            Color glowColor = new Color(110, 0, 200) * 0.6f;

            Main.spriteBatch.Draw(glowTex, textCenter, null, glowColor, 0f, glowTex.Size() / 2f, new Vector2(glowScale, glowScale * 0.5f), SpriteEffects.None, 0f);
            int particleCount = Math.Max(8, (int)(textSize.X / 10f));

            for (int i = 0; i < particleCount; i++)
            {
                float timeOffset = i * 1.618f;
                float time = Main.GlobalTimeWrappedHourly * 1.2f + timeOffset;

                float progress = time % 1f;

                float angle = (i * MathHelper.TwoPi / particleCount) + (Main.GlobalTimeWrappedHourly * 0.5f);
                float radiusX = textSize.X * 0.8f;
                float radiusY = textSize.Y * 1.5f;
                float currentRadius = 1f - progress;

                float currentX = textCenter.X + (float)Math.Cos(angle) * radiusX * currentRadius;
                float currentY = textCenter.Y + (float)Math.Sin(angle) * radiusY * currentRadius;

                float alpha = progress < 0.2f ? (progress / 0.2f) : (1f - progress) * 1.2f;
                alpha = MathHelper.Clamp(alpha, 0f, 1f);

                float scale = 0.5f + (currentRadius * 0.5f);
                Color drawColor = new Color(150, 50, 255) * alpha;
                Main.spriteBatch.Draw(cubeTex, new Vector2(currentX, currentY), null, drawColor, time * 2f, cubeTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private void DrawStormEffect(Item item, DrawableTooltipLine line)
        {
            Texture2D sparkTex = ModContent.Request<Texture2D>("jarkos/particles/Spark").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);

            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float flash = (float)Math.Pow(Math.Sin(Main.GlobalTimeWrappedHourly * 25f), 4);

            float glowScale = textSize.X / glowTex.Width * 1.2f * (0.8f + flash * 0.4f);
            Color glowColor = new Color(255, 50, 180) * (0.35f + flash * 0.4f);

            Main.spriteBatch.Draw(glowTex, textCenter, null, glowColor, 0f, glowTex.Size() / 2f, new Vector2(glowScale, glowScale * 0.5f), SpriteEffects.None, 0f);
            int sparkCount = 6;

            for (int i = 0; i < sparkCount; i++)
            {
                float timeOffset = i * 2.33f;
                float snappedTime = (float)Math.Floor(Main.GlobalTimeWrappedHourly * 8f + timeOffset);
                float seedX = snappedTime * (i + 2);
                float seedY = snappedTime * (i + 3);

                float currentX = line.X + (seedX % textSize.X);
                float currentY = line.Y + (seedY % textSize.Y);

                float rotation = seedX % MathHelper.TwoPi;
                float scale = 0.5f + (seedY % 1.5f);
                float alpha = (seedX % 100f) > 30f ? 0.9f : 0f;

                Color drawColor = new Color(255, 170, 230) * alpha;
                Main.spriteBatch.Draw(sparkTex, new Vector2(currentX, currentY), null, drawColor, rotation, sparkTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
    }
}
