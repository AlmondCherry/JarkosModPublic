using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
namespace jarkos.UI
{
    public class RuneChargerUISystem : ModSystem
    {
        public static RuneChargerUI ChargerUI;
        public override void Load()
        {
            ChargerUI = new RuneChargerUI();
        }
        public override void Unload()
        {
            ChargerUI = null;
        }
        public static void OpenUI()
        {
            var rune = Main.LocalPlayer.HeldItem.ModItem as jarkos.items.BaseRuneItem;
            if (rune != null)
            {
                ChargerUI.Open(rune);
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (ChargerUI != null && ChargerUI.IsOpen)
            {
                ChargerUI.Update(gameTime);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("jarkos: RuneChargerUI", delegate
                    {
                            if (ChargerUI != null && ChargerUI.IsOpen)
                        {
                            ChargerUI.Draw(Main.spriteBatch);
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
}
