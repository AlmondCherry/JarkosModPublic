using jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.small_wooden
{
	[AutoloadEquip(EquipType.Legs)]
	public class Weak_wooden_leggins : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 1;
        }
        public override void UpdateEquip(Player player)
        {
           player.moveSpeed += 0.05f;
           player.GetDamage(DamageClass.Summon) += 0.02f;
        }
        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 50)
            .AddIngredient(ModContent.ItemType<FragmentOfNature>(), 3)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
    }
}
