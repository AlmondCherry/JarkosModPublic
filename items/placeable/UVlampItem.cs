using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.placeable
{
    public class UVlampItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<tiles.UVlampTile>());
            Item.width = 42;
            Item.height = 46;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 99;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Glass, 10)
                .AddIngredient(ItemID.Amethyst, 3)
                .AddIngredient(ItemID.Wire, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
