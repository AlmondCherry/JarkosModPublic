using jarkos.items.materials;
using jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.crystal_clear_alloys
{
    [AutoloadEquip(EquipType.Body)]
    public class CrystalClearAlloy_breast : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 26;
            Item.defense = 30;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 15, 0, 0);
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.GetDamage(DamageClass.Magic) += 0.15f;
            player.GetCritChance(DamageClass.Ranged) += 7;
            player.GetCritChance(DamageClass.Magic) += 7;
            player.statManaMax2 += 60;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<CrystalClearAlloy>(), 2)
                .AddIngredient(ModContent.ItemType<LunarCrystal>(), 12)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
