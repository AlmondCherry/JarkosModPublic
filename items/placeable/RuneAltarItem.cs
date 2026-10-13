using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.tiles;

namespace jarkos.items.placeable
{
    public class RuneAltarItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<RuneAltarTile>();
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
        }

        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 200)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 20)
            .AddIngredient(ItemID.Diamond, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
