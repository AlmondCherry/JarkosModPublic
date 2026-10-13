using jarkos.items.usetiles;
using jarkos.rare;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class AncientFragment : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ModContent.RarityType<SpeedRarity>();
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 1f, 1f);
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 1)
			.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 2)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 2)
			.AddIngredient(ModContent.ItemType<EnchantedFragment>(), 2)
			.Register();
		}
	}
}
