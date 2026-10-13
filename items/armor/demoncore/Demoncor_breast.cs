using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace jarkos.items.armor.demoncore
{
	[AutoloadEquip(EquipType.Body)]
	public class Demoncor_breast : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 26;
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
            player.endurance += 0.05f;
            player.lifeRegen += 3;
            player.manaRegen -= 5;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 6)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 20)
			.AddIngredient(ItemID.HellstoneBar, 40)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
