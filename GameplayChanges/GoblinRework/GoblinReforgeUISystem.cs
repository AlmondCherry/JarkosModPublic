using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.GameplayChanges.GoblinRework
{
    public class GoblinReforgeUISystem : ModSystem
    {
        public UserInterface ReforgeUserInterface;
        public GoblinReforgeUIState ReforgeUIState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                ReforgeUserInterface = new UserInterface();
                ReforgeUIState = new GoblinReforgeUIState();
                ReforgeUIState.Activate();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!Jarkos_config.GoblinReforgeEnabled)
            {
                CloseUI();
                return;
            }

            Player player = Main.LocalPlayer;

            if (player.talkNPC >= 0 && Main.npc[player.talkNPC].type == NPCID.GoblinTinkerer)
            {
                Main.InReforgeMenu = false;
                if (Main.reforgeItem != null && !Main.reforgeItem.IsAir)
                {
                    Main.reforgeItem.SetDefaults(ItemID.None);
                }
            }

            if (ReforgeUserInterface?.CurrentState != null)
            {
                if (!Main.playerInventory || player.talkNPC < 0 || Main.npc[player.talkNPC].type != NPCID.GoblinTinkerer)
                {
                    CloseUI();
                }
                else
                {
                    ReforgeUserInterface.Update(gameTime);
                }
            }
            else
            {
                if (Main.playerInventory && player.talkNPC >= 0 && Main.npc[player.talkNPC].type == NPCID.GoblinTinkerer)
                {
                    OpenUI();
                }
            }
        }

        public void OpenUI()
        {
            if (!Jarkos_config.GoblinReforgeEnabled)
                return;

            if (ReforgeUserInterface != null && ReforgeUserInterface.CurrentState == null)
            {
                ReforgeUserInterface.SetState(ReforgeUIState);
            }
        }

        public void CloseUI()
        {
            if (ReforgeUserInterface != null && ReforgeUserInterface.CurrentState != null)
            {
                if (ReforgeUIState.ReforgeItem != null && !ReforgeUIState.ReforgeItem.IsAir)
                {
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("GoblinReforge"), ReforgeUIState.ReforgeItem, ReforgeUIState.ReforgeItem.stack);
                    ReforgeUIState.ReforgeItem.SetDefaults(ItemID.None);
                }

                ReforgeUserInterface.SetState(null);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!Jarkos_config.GoblinReforgeEnabled)
                return;

            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "jarkos: Goblin Reforge UI",
                    delegate
                    {
                        if (ReforgeUserInterface?.CurrentState != null)
                        {
                            ReforgeUserInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
    public class GoblinTinkererReforgeOverrideNPC : GlobalNPC
    {
        public override void OnChatButtonClicked(NPC npc, bool firstButton)
        {
            if (npc.type == NPCID.GoblinTinkerer && !firstButton)
            {
                if (Jarkos_config.GoblinReforgeEnabled)
                {
                    Main.InReforgeMenu = false;
                    ModContent.GetInstance<GoblinReforgeUISystem>().OpenUI();
                }
            }
        }
    }
}
