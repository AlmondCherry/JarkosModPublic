using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.placeable
{
    public class GoldenTrophyItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<tiles.GoldenTrophyTile>());
            Item.width = 32;
            Item.height = 48;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
