using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
    public class SpectralFuelItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Ectoplasm;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void AddRecipes()
        {
            CreateRecipe(5)
                .AddIngredient(ItemID.Ectoplasm, 1)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
