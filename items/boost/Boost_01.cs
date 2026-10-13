using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.boost
{
	public class Boost_01 : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 20;
			Item.useTime = 32;
			Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.HoldUp;
			Item.scale = 1f;
			Item.value = Item.buyPrice(silver: 50);
			Item.consumable = true;
            Item.maxStack = 1;
        }
		public override bool CanUseItem(Player player) => 
		player.GetModPlayer<Jarkos_player_statboost>().DFFup < 1 && player.GetModPlayer<Jarkos_player_statboost>().DFFlimit == 1;
    	public override bool? UseItem(Player player)
    	{
			if (Main.myPlayer == player.whoAmI)

			player.HealEffect(25);

			++player.GetModPlayer<Jarkos_player_statboost>().DFFup;
			++player.GetModPlayer<Jarkos_player_statboost>().DFFlimit;
			player.GetModPlayer<Jarkos_player_statboost>().DFF_used = true;

			return new bool?(true);
    	}
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
			.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 8)
            .AddIngredient(ItemID.WhitePearl, 1)
            .AddTile(TileID.Anvils)
        	.Register();
		}
	}
}
