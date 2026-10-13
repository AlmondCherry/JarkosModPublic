using jarkos.items.materials;
using Jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.panel
{
	[AutoloadEquip(EquipType.Legs)]
	public class Panel_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 8;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.15f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<DiffusalBar>(), 4)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
