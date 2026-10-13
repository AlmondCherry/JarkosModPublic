using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.bloodmantle
{
	[AutoloadEquip(EquipType.Legs)]
	public class Bloodmantle_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 5;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.1f;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrimtaneBar, 10);
            recipe.AddIngredient(ItemID.TissueSample, 12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
