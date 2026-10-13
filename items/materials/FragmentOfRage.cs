using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class FragmentOfRage : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 40;
            Item.height = 40;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 20);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 1)
			.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 1)
			.Register();
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 1f, 1f);
        }
	}
}
