using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.tiles;
using Jarkos.items.materials;

namespace jarkos.items.placeable
{
    public class RuneChargerItem : ModItem
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
            Item.createTile = ModContent.TileType<RuneChargerTile>();
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 16)
			.AddIngredient(ModContent.ItemType<DiffusalBar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
