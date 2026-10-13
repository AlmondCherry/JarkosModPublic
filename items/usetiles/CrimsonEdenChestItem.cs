using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class CrimsonEdenChestItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Crimson_eden_Chest>());
			Item.width = 32;
			Item.height = 26;
			Item.value = 500;
		}
	}
	public class CE_ChestKey : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.GoldenKey);
		}
	}
}
