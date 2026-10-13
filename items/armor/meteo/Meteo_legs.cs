using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;

namespace jarkos.items.armor.meteo
{
	[AutoloadEquip(EquipType.Legs)]
	public class Meteo_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 14;
        }
        public override void UpdateEquip(Player player)
        {
           player.moveSpeed += 0.25f;
           player.GetDamage(DamageClass.Magic) += 0.05f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.MeteoriteBar, 16)
			.AddRecipeGroup(nameof(ItemID.TitaniumBar), 8)
            .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
