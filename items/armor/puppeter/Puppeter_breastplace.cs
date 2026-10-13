using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.puppeter
{
	[AutoloadEquip(EquipType.Body)]
	public class Puppeter_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 26;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 14;
        }
        public override void UpdateEquip(Player player)
        {
            player.maxMinions += 1;
            player.GetDamage(DamageClass.Summon) += 0.10f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 12)
                .AddIngredient(ItemID.Silk, 18)
                .AddIngredient(ItemID.SpookyWood, 30)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
