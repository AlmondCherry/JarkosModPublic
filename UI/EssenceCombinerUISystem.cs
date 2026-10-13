using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.UI
{
    public class EssenceCombinerUISystem : ModSystem
    {
        public static EssenceCombinerUI CombinerUI;
        private UserInterface _combinerInterface;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                CombinerUI = new EssenceCombinerUI();
                CombinerUI.Activate();
                _combinerInterface = new UserInterface();
                _combinerInterface.SetState(CombinerUI);
            }
        }
        public override void Unload()
        {
            CombinerUI = null;
            _combinerInterface = null;
        }
        public static void OpenUI()
        {
            if (CombinerUI != null)
            {
                CombinerUI.Open();
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (CombinerUI != null && CombinerUI.IsOpen)
            {
                _combinerInterface?.Update(gameTime);
                CombinerUI.UpdateAnimation();
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("jarkos: EssenceCombinerUI", delegate
                    {
                        if (CombinerUI != null && CombinerUI.IsOpen)
                        {
                            _combinerInterface?.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
}
