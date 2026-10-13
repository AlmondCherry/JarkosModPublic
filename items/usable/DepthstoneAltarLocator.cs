using jarkos.cradle_biome;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
	public class DepthstoneAltarLocator : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTurn = true;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.LightRed;
			Item.maxStack = 1;
			Item.holdStyle = ItemHoldStyleID.HoldFront;
		}
		public override void HoldItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
				return;
				
			AltarLocatorSystem.MarkLocatorHeld();
		}
	}
}
