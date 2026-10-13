using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.puppeter
{
	[AutoloadEquip(EquipType.Legs)]
	public class Puppeter_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 26;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 9;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.08f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 6)
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.SpookyWood, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
