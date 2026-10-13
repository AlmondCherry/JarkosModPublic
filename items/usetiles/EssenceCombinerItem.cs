using jarkos.items.materials;
using jarkos.tiles;
using Jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
    public class EssenceCombinerItem : ModItem
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
            Item.value = Item.buyPrice(silver: 25);
            Item.createTile = ModContent.TileType<EssenceCombinerTile>();
        }

        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 120)
			.AddIngredient(ModContent.ItemType<DiffusalBar>(), 8)
            .AddIngredient(ModContent.ItemType<ThickSilk>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
