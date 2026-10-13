using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.archivist
{
	[AutoloadEquip(EquipType.Legs)]
	public class Archivist_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 7);
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 12;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.10f;
            player.manaRegenBonus += 5;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 16)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
