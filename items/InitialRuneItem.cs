using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.items.materials;

namespace jarkos.items
{
    public class InitialRuneItem : BaseRuneItem
    {
        public override int MaxCapacity => 100;
        public override int MaxSlots => 2;

        public override void SafeSetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
        }

        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.HellstoneBar, 5)
			.AddIngredient(ModContent.ItemType<EssentialPowder>(), 2)
            .AddIngredient(ModContent.ItemType<EnchantedFragment>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
