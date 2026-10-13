using jarkos.tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
    public class EssenceSpreaderItem : ModItem
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
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.createTile = ModContent.TileType<EssenceSpreaderTile>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddIngredient(ItemID.IronBar, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
