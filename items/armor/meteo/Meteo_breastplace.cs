using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;

namespace jarkos.items.armor.meteo
{
	[AutoloadEquip(EquipType.Body)]
	public class Meteo_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 26;
            Item.defense = 18;
        }
        public override void UpdateEquip(Player player)
        {
           player.GetDamage(DamageClass.Magic) += 0.15f;
           player.GetArmorPenetration(DamageClass.Magic) += 20;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.MeteoriteBar, 24)
			.AddRecipeGroup(nameof(ItemID.TitaniumBar), 12)
            .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
