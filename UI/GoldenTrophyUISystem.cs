using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.UI
{
    public class GoldenTrophyUISystem : ModSystem
    {
        public static GoldenTrophyUI TrophyUI;

        public override void Load()
        {
            TrophyUI = new GoldenTrophyUI();
        }

        public override void Unload()
        {
            TrophyUI = null;
        }

        public static void OpenUI(Point? tileCoords = null)
        {
            TrophyUI.Open(tileCoords);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (TrophyUI != null && TrophyUI.IsOpen)
            {
                TrophyUI.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("jarkos: GoldenTrophyUI", delegate
                {
                    if (TrophyUI != null && TrophyUI.IsOpen)
                    {
                        TrophyUI.Draw(Main.spriteBatch);
                    }
                    return true;
                }, InterfaceScaleType.UI));
            }
        }
    }
}
