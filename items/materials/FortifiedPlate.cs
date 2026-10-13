using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.items.materials;

namespace jarkos.items.materials
{
	public class FortifiedPlate : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 30;
            Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<GoldenCore>(), 1)
			.AddIngredient(ItemID.SoulofMight, 1)
			.AddIngredient(ItemID.SoulofSight, 1)
			.AddIngredient(ItemID.SoulofFright, 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
