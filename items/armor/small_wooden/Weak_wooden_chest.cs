using jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.small_wooden
{
	[AutoloadEquip(EquipType.Body)]
	public class Weak_wooden_chest : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 26;
            Item.defense = 2;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetAttackSpeed(DamageClass.Summon) += 0.05f;
        }
         public override void AddRecipes()
         {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 150)
            .AddIngredient(ModContent.ItemType<FragmentOfNature>(), 10)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
    }
}
