using System.Collections.Generic;
using jarkos.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.core
{
    public class ResearchUISystem : ModSystem
    {
        public static int CloseCooldown = 0;
        public override void PreUpdateEntities()
        {
            if (CloseCooldown > 0) CloseCooldown--;
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (!ResearchScreen.Instance.Active) return;
            ResearchScreen.Instance.Update(gameTime, Main.LocalPlayer);
            Main.LocalPlayer.mouseInterface = true;
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!ResearchScreen.Instance.Active) return;
            foreach (var layer in layers)
            {
                if (layer.Name != "Vanilla: Cursor" && layer.Name != "Vanilla: Mouse Text")
                {
                    layer.Active = false;
                }
            }
            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (idx == -1) idx = layers.Count - 1;
            layers.Insert(idx, new LegacyGameInterfaceLayer("ResearchTable: Constellation Screen", () =>
                {
                    ResearchScreen.Instance.Draw(Main.spriteBatch, Main.LocalPlayer);
                    return true;
                }, InterfaceScaleType.UI));
        }
    }
    public class ResearchKeyPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggers)
        {
            if (ResearchScreen.Instance.Active)
            {
                triggers.Inventory = false;
                PlayerInput.Triggers.Current.Inventory = false;
                PlayerInput.Triggers.JustPressed.Inventory = false;
            }
        }
    }
}
