using jarkos.items.materials;
using Jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.panel
{
	[AutoloadEquip(EquipType.Body)]
	public class Panel_breast : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 26;
            Item.defense = 12;
            
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.06f;
            player.GetCritChance(DamageClass.Generic) += 5f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<DiffusalBar>(), 8)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
