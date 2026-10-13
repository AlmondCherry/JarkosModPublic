using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.archivist
{
	[AutoloadEquip(EquipType.Body)]
	public class Archivist_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 7);
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 16;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Magic) += 12f;
            player.manaCost -= 0.10f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
