using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using jarkos.core.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;

namespace jarkos.items
{
    public class EnchantedArmorGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public bool IsEnchantedByRune = false;
        public Dictionary<EssenceType, int> Essences = new Dictionary<EssenceType, int>();

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.headSlot > 0 || entity.bodySlot > 0 || entity.legSlot > 0;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (IsEnchantedByRune)
            {
                tag["IsEnchantedByRune"] = true;
                List<TagCompound> essenceList = new List<TagCompound>();

                foreach (var kvp in Essences)
                {
                    essenceList.Add(new TagCompound()
                    {
                        ["Type"] = (int)kvp.Key, ["Amount"] = kvp.Value
                    });
                }
                tag["Essences"] = essenceList;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            IsEnchantedByRune = tag.ContainsKey("IsEnchantedByRune") && tag.GetBool("IsEnchantedByRune");
            Essences.Clear();

            if (tag.ContainsKey("Essences"))
            {
                var list = tag.GetList<TagCompound>("Essences");
                foreach (var elem in list)
                {
                    EssenceType type = (EssenceType)elem.GetInt("Type");
                    int amount = elem.GetInt("Amount");
                    Essences[type] = amount;
                }
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (IsEnchantedByRune)
            {
                foreach (var line in tooltips)
                {
                    if (line.Mod == "Terraria" && line.Name == "ItemName")
                    {
                        line.Text = "Enchanted " + line.Text;
                        line.OverrideColor = Color.LightGoldenrodYellow;
                        break;
                    }
                }
                tooltips.Add(new TooltipLine(Mod, "RuneEnchantedInfo", "This is enchanted with essential runes")
                {
                    OverrideColor = Color.Gold
                });

                foreach (var kvp in Essences)
                {
                    string bonus = EssencePalette.GetBonusText(kvp.Key, kvp.Value);
                    string text = string.IsNullOrEmpty(bonus) ? $"+{kvp.Value} {kvp.Key}" : $"+{kvp.Value} {kvp.Key} ({bonus})";

                    tooltips.Add(new TooltipLine(Mod, "RuneEssence_" + kvp.Key.ToString(), text)
                    {
                        OverrideColor = EssencePalette.ColorOf(kvp.Key)
                    });
                }
            }
        }

        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            if (IsEnchantedByRune && line.Mod == Mod.Name && line.Name == "RuneEnchantedInfo")
            {
                DrawRuneEnchantedInfoEffect(line);
            }
            return true;
        }

        private void DrawRuneEnchantedInfoEffect(DrawableTooltipLine line)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Texture2D particleTex = ModContent.Request<Texture2D>("jarkos/particles/Solid_particle").Value;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(line.Text);
            Vector2 textCenter = new Vector2(line.X + textSize.X / 2f, line.Y + textSize.Y / 2f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float glowPulse = 0.75f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.25f;
            Color glowColor = new Color(255, 215, 80) * 0.5f * glowPulse;
            Vector2 glowScaleVec = new Vector2(textSize.X / glowTex.Width * 1.3f, 0.45f * glowPulse);

            Main.spriteBatch.Draw(glowTex, textCenter, null, glowColor, 0f, glowTex.Size() / 2f, glowScaleVec, SpriteEffects.None, 0f);
            
            int particleCount = 14;

            for (int i = 0; i < particleCount; i++)
            {
                float timeOffset = i * 1.73f;
                float time = Main.GlobalTimeWrappedHourly * 0.8f + timeOffset;
                float progress = time % 1f;

                float posX = line.X + ((i * 37f + 11f) % textSize.X);
                float startY = line.Y + textSize.Y / 2f + 4f;
                float posY = startY - (progress * 24f);

                float alpha = (float)Math.Sin(progress * MathHelper.Pi);
                float scale = (0.22f + i % 3 * 0.1f) * (0.6f + alpha * 0.4f);

                Color particleColor = Color.Lerp(new Color(255, 220, 100), new Color(255, 140, 40), (i % 4) / 4f) * alpha;

                Main.spriteBatch.Draw(particleTex, new Vector2(posX, posY), null, particleColor, progress * MathHelper.TwoPi, particleTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
    }
}
