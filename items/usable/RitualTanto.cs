using jarkos.npc.Punisher.Cutscene;
using Jarkos;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
    public class RitualTanto : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 34;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.maxStack = 1;
            Item.consumable = false;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 1);
            Item.UseSound = null;
            Item.noUseGraphic = true;
        }
        public override bool CanUseItem(Player player)
        {
            var cp = player.GetModPlayer<HarakiriCutscenePlayer>();
            return !cp.CutsceneActive && !NPC.AnyNPCs(HarakiriCutscenePlayer.BossToSummon);
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.GetModPlayer<HarakiriCutscenePlayer>().StartCutscene();
                
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)JarkosPacketType.HarakiriCutsceneSync);
                    packet.Write((byte)player.whoAmI);
                    packet.Send();
                }
            }
            return true;
        }
    }
}
