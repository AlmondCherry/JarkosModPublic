using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using jarkos.items.materials;
using jarkos.npc.godofdeath;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.weapons.magic.staffs;
using jarkos.items.weapons.ranged.firing;

namespace jarkos.items.bags
{
    public class Godofdeath_bag : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 34;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.expert = true;
        }
        public override bool CanRightClick()
        {
            return true;
        }
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<FeatherBlack>(), 1, 4, 12));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Plague_slimemachine>(), 20, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<KnifeSacrifice>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Plague_magestaff>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpearOfDeath>(), 4, 1, 1));
			itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<God_of_death>()));
		}
    }
}
