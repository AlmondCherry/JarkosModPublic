using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace jarkos.GameplayChanges
{
    public class DemolitionistGemShop : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Demolitionist)
            {
                int gemPrice = Item.buyPrice(0, 0, 50, 0); 

                shop.Add(new Item(ItemID.Amethyst) { shopCustomPrice = gemPrice },
                    new Condition(Language.GetOrRegister("Mods.jarkos.Conditions.DiscoveredAmethyst", () => "Found Amethyst"),
                    () => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<Jarkos_player>().discoveredAmethyst));

                shop.Add(new Item(ItemID.Topaz) { shopCustomPrice = gemPrice },
                    new Condition(Language.GetOrRegister("Mods.jarkos.Conditions.DiscoveredTopaz", () => "Found Topaz"),
                    () => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<Jarkos_player>().discoveredTopaz));

                shop.Add(new Item(ItemID.Sapphire) { shopCustomPrice = gemPrice },
                    new Condition(Language.GetOrRegister("Mods.jarkos.Conditions.DiscoveredSapphire", () => "Found Sapphire"),
                    () => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<Jarkos_player>().discoveredSapphire));

                shop.Add(new Item(ItemID.Emerald) { shopCustomPrice = gemPrice },
                    new Condition(Language.GetOrRegister("Mods.jarkos.Conditions.DiscoveredEmerald", () => "Found Emerald"),
                    () => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<Jarkos_player>().discoveredEmerald));

                shop.Add(new Item(ItemID.Ruby) { shopCustomPrice = gemPrice },
                    new Condition(Language.GetOrRegister("Mods.jarkos.Conditions.DiscoveredRuby", () => "Found Ruby"),
                    () => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<Jarkos_player>().discoveredRuby));

                shop.Add(new Item(ItemID.Diamond) { shopCustomPrice = gemPrice },
                    new Condition(Language.GetOrRegister("Mods.jarkos.Conditions.DiscoveredDiamond", () => "Found Diamond"),
                    () => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<Jarkos_player>().discoveredDiamond));
            }
        }
    }
}
