using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.npc.CradleMobs;

namespace jarkos.items.materials
{
	public class CradleWormItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.bait = 30;
			Item.makeNPC = (short)ModContent.NPCType<CradleWorm>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 10, 0);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
