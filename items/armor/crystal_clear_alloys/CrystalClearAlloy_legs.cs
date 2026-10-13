using jarkos.items.materials;
using jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.crystal_clear_alloys
{
    [AutoloadEquip(EquipType.Legs)]
    public class CrystalClearAlloy_legs : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 26;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 12, 0, 0);
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 0.10f;
            player.GetDamage(DamageClass.Magic) += 0.10f;
            player.GetCritChance(DamageClass.Ranged) += 5;
            player.GetCritChance(DamageClass.Magic) += 5;
            player.moveSpeed += 0.25f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<CrystalClearAlloy>(), 1)
                .AddIngredient(ModContent.ItemType<LunarCrystal>(), 6)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
