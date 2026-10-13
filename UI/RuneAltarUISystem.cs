using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
namespace jarkos.UI
{
    public class RuneAltarUISystem : ModSystem
    {
        public static RuneAltarUI AltarUI;
        public override void Load()
        {
            AltarUI = new RuneAltarUI();
        }
        public override void Unload()
        {
            AltarUI = null;
        }
        public static void OpenUI(Point? tileCoords = null)
        {
            AltarUI.Open(null, tileCoords);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (AltarUI != null && AltarUI.IsOpen)
            {
                AltarUI.Update(gameTime);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("jarkos: RuneAltarUI", delegate
                    {
                        if (AltarUI != null && AltarUI.IsOpen)
                        {
                            AltarUI.Draw(Main.spriteBatch);
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
}
