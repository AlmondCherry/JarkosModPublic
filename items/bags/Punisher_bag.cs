using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using jarkos.items.weapons.melee.true_melee;
using jarkos.items.accs.general;
using jarkos.npc.Punisher;
using jarkos.items.accs.melee;
using jarkos.items.weapons.ranged.bows;
using jarkos.items.weapons.summon.staff.PhantomDagger;
using jarkos.items.weapons.summon.whip;
using jarkos.items.weapons.melee.unique_melee;

namespace jarkos.items.bags
{
    public class Punisher_bag : ModItem
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
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Mage_slayer>(), 2, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SealOfShadow>(), 2, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Heavenly_flame>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<PhantomDagger_item>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TemporalFissure>(), 2, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<KarmaBlade>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Vine_of_Rage>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Silent_destroyer>(), 2, 1, 1));
			itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<Punisher>()));
		}
    }
}
