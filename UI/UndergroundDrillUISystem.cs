using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.tiles;
using Terraria.Audio;
using Terraria.ID;

namespace jarkos.UI
{
    public class UndergroundDrillUISystem : ModSystem
    {
        public static UndergroundDrillUISystem Instance => ModContent.GetInstance<UndergroundDrillUISystem>();

        private UserInterface _drillUserInterface;
        public UndergroundDrillUIState DrillUIState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _drillUserInterface = new UserInterface();
                DrillUIState = new UndergroundDrillUIState();
                DrillUIState.Activate();
            }
        }

        public override void Unload()
        {
            DrillUIState = null;
            _drillUserInterface = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (_drillUserInterface?.CurrentState != null)
            {
                _drillUserInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "jarkos: Underground Drill UI",
                    delegate
                    {
                        if (_drillUserInterface?.CurrentState != null)
                        {
                            _drillUserInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public void OpenUI(UndergroundDrillTileEntity drillEntity)
        {
            DrillUIState.SetEntity(drillEntity);
            DrillUIState.ResetAnimation();
            _drillUserInterface.SetState(DrillUIState);
            Main.playerInventory = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public void CloseUI()
        {
            if (_drillUserInterface?.CurrentState != null)
            {
                if (DrillUIState != null && !DrillUIState.IsClosing)
                {
                    DrillUIState.IsClosing = true;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
            }
        }

        public void FinishCloseUI()
        {
            if (_drillUserInterface?.CurrentState != null)
            {
                _drillUserInterface.SetState(null);
            }
        }

        public bool IsUIOpen()
        {
            return _drillUserInterface?.CurrentState != null;
        }
    }
}
