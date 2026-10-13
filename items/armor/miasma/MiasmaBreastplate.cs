using jarkos.items.materials;
using jarkos.rare;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.miasma
{
    [AutoloadEquip(EquipType.Body)]
    public class MiasmaBreastplate : ModItem
    {
		public override void SetDefaults()
        {
			Item.width = 30;
			Item.height = 20;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ModContent.RarityType<StormRarity>();
			Item.defense = 16;
		}
		public override void UpdateEquip(Player player)
        {
			player.GetCritChance(DamageClass.Generic) += 6;
			player.statLifeMax2 += 20;
		}
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 8)
                .AddIngredient(ModContent.ItemType<Vileplasm>(), 3)
                .AddIngredient(ModContent.ItemType<BulbSunEnergy>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
