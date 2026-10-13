using jarkos.projectiles.other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class EssenceJarItem : ModItem
	{
		public override string Texture => "jarkos/items/potions/Strength_Essence";

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<EssenceJarProjectile>();
			Item.shootSpeed = 10f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 15, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ItemID.Bottle, 1)
				.AddIngredient(ItemID.FallenStar, 2)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
