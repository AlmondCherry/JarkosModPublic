using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using jarkos.items.materials;
using jarkos.npc.tenderote;
using jarkos.items.weapons.magic.toms;
using jarkos.items.usetiles;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.weapons.ranged.firing;

namespace jarkos.items.bags
{
    public class Tenderot_bag : ModItem
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
			itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<TenderoteMechanism>(), 7));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TenderoteMechanism>(), 1, 4, 8));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<AntiqueOre>(), 1, 38, 102));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlackHole>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Antique_melee_axe>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Antique_beam_rifle>(), 4, 1, 1));
			itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<Tenderote_head>()));
		}
    }
}
