using jarkos.items.materials;
using jarkos.rare;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.anker
{
	[AutoloadEquip(EquipType.Legs)]
	public class Anker_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<CosmicRarity>();
            Item.defense = 28;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.15f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.VortexLeggings, 1)
                .AddIngredient(ModContent.ItemType<HeavenlyPowder>(), 2)
                .AddIngredient(ModContent.ItemType<CelestiaStar>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
