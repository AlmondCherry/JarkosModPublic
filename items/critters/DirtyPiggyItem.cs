using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.npc.CradleMobs;

namespace jarkos.items.critters
{
	public class DirtyPiggyItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 5;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 24;
			Item.maxStack = 9999;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.consumable = true;
			Item.noMelee = true;
			Item.value = Item.buyPrice(0, 0, 70, 95);
			Item.rare = ItemRarityID.White;
			Item.makeNPC = (short)ModContent.NPCType<DirtyPiggy>();
		}
	}
}
