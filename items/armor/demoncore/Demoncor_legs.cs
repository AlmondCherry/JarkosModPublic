using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace jarkos.items.armor.demoncore
{
	[AutoloadEquip(EquipType.Legs)]
	public class Demoncor_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
           player.moveSpeed += 0.08f;
           player.accRunSpeed += 0.08f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 2)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 5)
			.AddIngredient(ItemID.HellstoneBar, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
