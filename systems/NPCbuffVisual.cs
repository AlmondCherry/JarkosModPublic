using System;
using System.Collections.Generic;
using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace jarkos.systems
{
    public class NPCbuffVisualUISystem : ModSystem
    {
        public static string HoveredBuffName = null;
        public static string HoveredBuffDesc = null;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex + 1, new LegacyGameInterfaceLayer("Jarkos: UI", delegate
                {
                    if (!string.IsNullOrEmpty(HoveredBuffName))
                    {
                        DrawBuffTooltip(Main.spriteBatch, HoveredBuffName, HoveredBuffDesc);
                        HoveredBuffName = null;
                        HoveredBuffDesc = null;
                    }
                    return true;
                }, InterfaceScaleType.UI));
            }
        }

        public static void DrawBuffTooltip(SpriteBatch spriteBatch, string name, string desc)
        {
            if (!Jarkos_config.Instance.EnableBuffNPCVisual) return;

            var font = FontAssets.MouseText.Value;
            float maxWidth = 280f;

            if (!string.IsNullOrEmpty(desc) && (desc.StartsWith("Mods.") || desc.EndsWith(".Description")))
            {
                desc = "";
            }

            string wrappedDesc = "";
            if (!string.IsNullOrEmpty(desc))
            {
                string[] words = desc.Split(' ');
                string currentLine = "";
                foreach (string word in words)
                {
                    if (font.MeasureString(currentLine + word).X * 0.8f > maxWidth)
                    {
                        wrappedDesc += currentLine.TrimEnd() + "\n";
                        currentLine = word + " ";
                    }
                    else
                    {
                        currentLine += word + " ";
                    }
                }
                wrappedDesc += currentLine.TrimEnd();
            }

            bool hasDesc = !string.IsNullOrEmpty(wrappedDesc);

            Vector2 nameSize = font.MeasureString(name) * 0.9f;
            Vector2 descSize = hasDesc ? font.MeasureString(wrappedDesc) * 0.8f : Vector2.Zero;

            float tooltipWidth = Math.Max(nameSize.X, descSize.X) + 24f;
            float titleHeight = 22f;
            float tooltipHeight = titleHeight + (hasDesc ? descSize.Y + 4f : 0f) + 16f;

            Vector2 mousePos = Main.MouseScreen + new Vector2(16, 16);

            if (mousePos.X + tooltipWidth > Main.screenWidth - 10)
                mousePos.X = Main.screenWidth - tooltipWidth - 10;
            if (mousePos.Y + tooltipHeight > Main.screenHeight - 10)
                mousePos.Y = Main.screenHeight - tooltipHeight - 10;

            Rectangle bgRect = new Rectangle((int)mousePos.X, (int)mousePos.Y, (int)tooltipWidth, (int)tooltipHeight);

            Terraria.Utils.DrawInvBG(spriteBatch, bgRect, new Color(12, 15, 30, (int)(255 * 0.30f)));

            Vector2 namePos = new Vector2(bgRect.X + 12, bgRect.Y + 8);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, name, namePos, Color.Gold, 0f, Vector2.Zero, new Vector2(0.9f));

            if (hasDesc)
            {
                Vector2 descPos = new Vector2(bgRect.X + 12, namePos.Y + titleHeight);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, wrappedDesc, descPos, Color.LightGray, 0f, Vector2.Zero, new Vector2(0.8f));
            }
        }
    }
    public class NPCbuffVisual : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private float TextureRot = 0f;

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!npc.active) return;
            if (!Jarkos_config.Instance.EnableBuffNPCVisual) return;

            List<int> activeBuffs = new List<int>();

            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                int type = npc.buffType[i];
                int time = npc.buffTime[i];
                if (type > 0 && time > 0 && !activeBuffs.Contains(type))
                {
                    activeBuffs.Add(type);
                }
            }

            int count = activeBuffs.Count;
            if (count == 0) return; 

            float scale = MathHelper.Clamp(0.75f / MathF.Sqrt(1f + (count - 1) * 0.25f), 0.28f, 0.75f); 

            int maxPerRow = count <= 4 ? 4 : (count <= 10 ? 6 : 8); 
            int totalRows = (int)MathF.Ceiling(count / (float)maxPerRow); 

            float iconDrawSize = 32f * scale;
            float spacing = MathF.Max(2f, 4f * scale);
            float stepX = iconDrawSize + spacing;
            float stepY = iconDrawSize + spacing;

            Vector2 headPos = new Vector2(npc.Center.X, npc.position.Y - 10f); 

            for (int r = 0; r < totalRows; r++) 
            {
                int startIndex = r * maxPerRow; 
                int TrulyInRow = Math.Min(maxPerRow, count - startIndex); 
                if (TrulyInRow <= 0) break;

                float rowWidth = TrulyInRow * iconDrawSize + (TrulyInRow - 1) * spacing;
                float startX = headPos.X - rowWidth / 2f + iconDrawSize / 2f;

                float rowY = headPos.Y - (totalRows - 1 - r) * stepY - iconDrawSize / 2f;

                for (int c = 0; c < TrulyInRow; c++)
                {
                    int buffType = activeBuffs[startIndex + c];

                    try
                    {
                        if (buffType <= 0 || buffType >= TextureAssets.Buff.Length) 
                            continue;

                        var buffAsset = TextureAssets.Buff[buffType];
                        if (buffAsset == null || buffAsset.Value == null)
                            continue;

                        Player player = Main.LocalPlayer;

                        Texture2D texture = buffAsset.Value;

                        Vector2 worldPos = new Vector2(startX + c * stepX, rowY);
                        Vector2 drawPos = worldPos - Main.screenPosition;
                        
                        Vector2 origin = texture.Size() / 2f;

                        bool reversed = player.gravDir < 1f;
                        TextureRot = reversed ? MathHelper.Pi : 0f;

                        spriteBatch.Draw(texture, drawPos + new Vector2(1.5f, 1.5f), null, Color.Black * 0.6f, TextureRot, origin, scale, SpriteEffects.None, 0f);
                        spriteBatch.Draw(texture, drawPos, null, Color.White, TextureRot, origin, scale, SpriteEffects.None, 0f);

                        Vector2 zoom = Main.GameViewMatrix.Zoom;

                        Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
                        Vector2 zoomedScreenPos = (drawPos - screenCenter) * zoom + screenCenter;

                        float zoomSizeIcon = iconDrawSize * zoom.Y;

                        Rectangle iconScreenRect = new Rectangle((int)(zoomedScreenPos.X - zoomSizeIcon / 2f), (int)(zoomedScreenPos.Y - zoomSizeIcon / 2f), (int)zoomSizeIcon, (int)zoomSizeIcon);

                        if (iconScreenRect.Contains(Main.MouseScreen.ToPoint()))
                        {
                            string bName = "";
                            string bDesc = "";

                            if (buffType < BuffID.Count)
                            {
                                bName = Lang.GetBuffName(buffType);
                                bDesc = Lang.GetBuffDescription(buffType);
                            }
                            else
                            {
                                ModBuff modBuff = BuffLoader.GetBuff(buffType);
                                if (modBuff != null)
                                {
                                    bName = modBuff.DisplayName.Value;
                                    bDesc = modBuff.Description.Value;
                                    if (modBuff is AutodafeDebuff autodafeDebuff)
                                    {
                                        //bDesc = autodafeDebuff.GetCustomDescription(npc);
                                    }
                                    else
                                    {
                                        int rare = 0;
                                        modBuff.ModifyBuffText(ref bName, ref bDesc, ref rare);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(bName))
                            {
                                NPCbuffVisualUISystem.HoveredBuffName = bName;
                                NPCbuffVisualUISystem.HoveredBuffDesc = bDesc;
                                
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
