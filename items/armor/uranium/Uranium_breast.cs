using jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.uranium
{
	[AutoloadEquip(EquipType.Body)]
	public class Uranium_breast : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 26;
            Item.defense = 24;
        }
        public override void UpdateEquip(Player player)
        {
            player.whipRangeMultiplier += 0.12f;
            player.GetAttackSpeed(DamageClass.Summon) += 0.12f;
        }
        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 12)
            .AddIngredient(ItemID.SoulofNight, 8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
