using jarkos.items.materials;
using jarkos.rare;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.anker
{
	[AutoloadEquip(EquipType.Body)]
	public class Anker_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<CosmicRarity>();
            Item.defense = 58;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 10f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.VortexBreastplate, 1)
                .AddIngredient(ModContent.ItemType<HeavenlyPowder>(), 2)
                .AddIngredient(ModContent.ItemType<CelestiaStar>(), 3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
