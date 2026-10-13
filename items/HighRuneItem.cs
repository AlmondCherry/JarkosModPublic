using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Jarkos.items.materials;

namespace jarkos.items
{
    public class HighRuneItem : BaseRuneItem
    {
        public override int MaxCapacity => 10000;
        public override int MaxSlots => 6;

        public override void SafeSetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<MidRuneItem>(), 1)
			.AddIngredient(ItemID.HallowedBar, 8)
			.AddIngredient(ModContent.ItemType<ResidualShadow>(), 6)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
