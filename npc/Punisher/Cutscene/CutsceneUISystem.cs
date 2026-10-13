using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
namespace jarkos.npc.Punisher.Cutscene
{
    public class CutsceneUISystem : ModSystem
    {
        private static readonly Color Violet = new Color(170, 80, 240);
        private static readonly Color DeepViolet = new Color(105, 25, 195);
        private static readonly Color PaleViolet = new Color(225, 190, 255);
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Add(new LegacyGameInterfaceLayer("Jarkos: Cutscene Overlay", DrawOverlay, InterfaceScaleType.None));
        }
        private static bool DrawOverlay()
        {
            if (Main.gameMenu || Main.dedServ)
                return true;
            HarakiriCutscenePlayer cp = null;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    var p = Main.player[i].GetModPlayer<HarakiriCutscenePlayer>();
                    if (p.CutsceneActive)
                    {
                        cp = p;
                        break;
                    }
                }
            }

            if (cp == null)
                return true;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int w = Main.screenWidth;
            int h = Main.screenHeight;
            float dark = cp.DarknessAlpha();
            if (dark > 0f)
                Main.spriteBatch.Draw(pixel, new Rectangle(0, 0, w, h), Color.Black * dark);
            float fl = cp.FlashAlpha();
            if (fl > 0.02f)
                Main.spriteBatch.Draw(pixel, new Rectangle(0, 0, w, h), Color.White * (fl * 0.85f));
            DrawBossTitle(cp, pixel, w, h);
            float lb = cp.LetterboxProgress();
            int barH = (int)(h * 0.12f * lb);
            if (barH > 0)
            {
                Main.spriteBatch.Draw(pixel, new Rectangle(0, 0, w, barH), Color.Black);
                Main.spriteBatch.Draw(pixel, new Rectangle(0, h - barH, w, barH), Color.Black);
            }
            return true;
        }
        private static void DrawBossTitle(HarakiriCutscenePlayer cp, Texture2D pixel, int w, int h)
        {
            const int START = HarakiriCutscenePlayer.BOSS_TICK + 50;
            const int LETTER_STAGGER = 7;
            const int LETTER_IN = 26;
            const int HOLD_END = HarakiriCutscenePlayer.BOSS_OUT_TICK + 40;
            const int GONE = HOLD_END + 40;
            int t = cp.Timer;
            if (t < START || t > GONE)
                return;
            DynamicSpriteFont font = FontAssets.DeathText.Value;
            string name = Lang.GetNPCNameValue(HarakiriCutscenePlayer.BossToSummon).ToUpper();
            if (string.IsNullOrWhiteSpace(name))
                return;
            const float baseScale = 1.2f;
            const float letterSpacing = 8f;
            int n = name.Length;
            float[] widths = new float[n];
            float total = 0f;
            for (int i = 0; i < n; i++)
            {
                widths[i] = font.MeasureString(name[i].ToString()).X * baseScale;
                total += widths[i];
            }
            total += letterSpacing * (n - 1);
            float baseY = h * 0.27f;
            float x = w / 2f - total / 2f;
            float fadeOut = 1f;
            float outRise = 0f;
            if (t > HOLD_END)
            {
                float o = (t - HOLD_END) / (float)(GONE - HOLD_END);
                fadeOut = 1f - o;
                outRise = -30f * o * o;
            }
            int lastLetterIn = START + (n - 1) * LETTER_STAGGER + LETTER_IN;
            float reveal = MathHelper.Clamp((t - START) / (float)(lastLetterIn - START), 0f, 1f);
            float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            int glowW = (int)(total * 1.25f);
            int glowH = (int)(font.MeasureString(name).Y * baseScale * 1.5f);
            Main.spriteBatch.Draw(pixel, new Rectangle(w / 2 - glowW / 2, (int)(baseY + outRise) - glowH / 2, glowW, glowH), DeepViolet * (0.10f * reveal * pulse * fadeOut));
            for (int i = 0; i < n; i++)
            {
                int lt = t - START - i * LETTER_STAGGER;
                if (lt <= 0)
                {
                    x += widths[i] + letterSpacing;
                    continue;
                }
                float p = Math.Min(lt / (float)LETTER_IN, 1f);
                float ease = 1f - (1f - p) * (1f - p) * (1f - p);
                float scale = baseScale * MathHelper.Lerp(2.1f, 1f, ease);
                float yOff = MathHelper.Lerp(-46f, 0f, ease) + outRise;
                float alpha = p * fadeOut;
                float rot = 0f;
                if (p >= 1f)
                {
                    yOff += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i * 0.7f) * 2.5f;
                    rot = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + i * 1.3f) * 0.035f;
                }
                Color fill = Color.Lerp(Color.White, Violet, Math.Min(lt / 16f, 1f));
                string ch = name[i].ToString();
                Vector2 sz = font.MeasureString(ch);
                Vector2 origin = sz / 2f;
                Vector2 pos = new Vector2(x + widths[i] / 2f, baseY + yOff);
                float glowA = 0.40f * alpha * (0.8f + 0.2f * pulse);
                for (int g = 0; g < 4; g++)
                {
                    Vector2 off = (MathHelper.PiOver2 * g + 0.7f).ToRotationVector2() * 3.5f;
                    Main.spriteBatch.DrawString(font, ch, pos + off, DeepViolet * glowA, rot, origin, scale, SpriteEffects.None, 0f);
                }
                Main.spriteBatch.DrawString(font, ch, pos + new Vector2(4f, 4f), Color.Black * (0.85f * alpha), rot, origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.DrawString(font, ch, pos, fill * alpha, rot, origin, scale, SpriteEffects.None, 0f);
                if (lt > LETTER_IN - 4 && lt < LETTER_IN + 10)
                {
                    float hit = 1f - Math.Abs(lt - LETTER_IN) / 10f;
                    Main.spriteBatch.DrawString(font, ch, pos, PaleViolet * (hit * 0.55f * fadeOut), rot, origin, scale * 1.06f, SpriteEffects.None, 0f);
                }
                x += widths[i] + letterSpacing;
            }
            if (t > lastLetterIn)
            {
                float lp = Math.Min((t - lastLetterIn) / 20f, 1f);
                float lEase = 1f - (1f - lp) * (1f - lp);
                int lineW = (int)(total * 0.92f * lEase);
                int lineY = (int)(baseY + font.MeasureString(name).Y * baseScale * 0.42f + outRise);
                Main.spriteBatch.Draw(pixel, new Rectangle(w / 2 - lineW / 2, lineY - 2, lineW, 6), DeepViolet * (0.45f * fadeOut * pulse));
                Main.spriteBatch.Draw(pixel, new Rectangle(w / 2 - lineW / 2, lineY, lineW, 2), PaleViolet * (0.95f * fadeOut));
                if (lp >= 1f)
                {
                    float sparkPos = (Main.GlobalTimeWrappedHourly * 1.4f) % 1f;
                    int sx = w / 2 - lineW / 2 + (int)(lineW * sparkPos);
                    Main.spriteBatch.Draw(pixel, new Rectangle(sx - 8, lineY - 1, 16, 4), Color.White * (0.8f * fadeOut));
                    int sx2 = w / 2 + lineW / 2 - (int)(lineW * sparkPos);
                    Main.spriteBatch.Draw(pixel, new Rectangle(sx2 - 8, lineY - 1, 16, 4), Color.White * (0.8f * fadeOut));
                }
            }
        }
    }
}
