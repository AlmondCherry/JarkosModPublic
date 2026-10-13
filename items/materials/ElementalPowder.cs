using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class ElementalPowder : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 0.9f, 0.4f);
        }

		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<EssentialPowder>(), 1)
			.AddIngredient(ItemID.CrystalShard, 3)
            .AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
