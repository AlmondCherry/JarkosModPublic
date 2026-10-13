using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.items.usetiles;
namespace jarkos.items
{
    public class MidRuneItem : BaseRuneItem
    {
        public override int MaxCapacity => 1000;
        public override int MaxSlots => 4;

        public override void SafeSetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(silver: 80);
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes() {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<InitialRuneItem>(), 1)
			.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 4)
            .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 6)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
