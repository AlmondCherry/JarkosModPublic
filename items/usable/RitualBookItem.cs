using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.UI;

namespace jarkos.items.usable
{
	public class RitualBookItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Green;
			Item.consumable = false;
			Item.UseSound = SoundID.Item1;
		}
		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				var uiSystem = ModContent.GetInstance<RitualBookUISystem>();
				if (uiSystem.IsBookOpen())
				{
					uiSystem.HideBook();
				}
				else
				{
					uiSystem.ShowBook();
				}
			}
			return true;
		}
		public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.Book, 1)
				.AddIngredient(ItemID.Silk, 5)
                .Register();
        }
	}
}
