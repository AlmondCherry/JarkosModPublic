using jarkos.items.materials;
using jarkos.rare;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.miasma
{
    [AutoloadEquip(EquipType.Legs)]
    public class MiasmaGreaves : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.defense = 14;
            Item.rare = ModContent.RarityType<StormRarity>();
            Item.value = Item.sellPrice(gold: 2);
        }
        public override void UpdateEquip(Player player)
        {
            player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
            player.manaCost -= 0.12f;
            player.moveSpeed += 0.10f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 4)
                .AddIngredient(ModContent.ItemType<Vileplasm>(), 1)
                .AddIngredient(ModContent.ItemType<BulbSunEnergy>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
