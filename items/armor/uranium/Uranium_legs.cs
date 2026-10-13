using jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.uranium
{
	[AutoloadEquip(EquipType.Legs)]
	public class Uranium_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 12;
        }
        public override void UpdateEquip(Player player)
        {
           player.maxMinions += 4;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 6)
            .AddIngredient(ItemID.SoulofNight, 4)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
