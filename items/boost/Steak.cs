using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;

namespace jarkos.items.boost
{
	public class Steak : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 42;
			Item.height = 40;
			Item.useTime = 32;
			Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.scale = 1f;
			Item.consumable = true;
            Item.maxStack = 1;
        }
		public override bool CanUseItem(Player player) => 
		player.GetModPlayer<Jarkos_player_statboost>().Steak_up < 1 && player.GetModPlayer<Jarkos_player_statboost>().Steaklimit == 1;
    	public override bool? UseItem(Player player)
    	{
      	if (Main.myPlayer == player.whoAmI)

        player.HealEffect(25);

      	++player.GetModPlayer<Jarkos_player_statboost>().Steak_up;
		++player.GetModPlayer<Jarkos_player_statboost>().Steaklimit;
		player.GetModPlayer<Jarkos_player_statboost>().Steak_used = true;

      	return new bool?(true);
    	}
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ItemID.LifeCrystal, 1)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 5)
            .AddIngredient(ItemID.SoulofLight, 5)
            .AddTile(TileID.MythrilAnvil)
        	.Register();
		}
	}
}
