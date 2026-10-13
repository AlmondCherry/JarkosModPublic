using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.hellabyssal
{
	[AutoloadEquip(EquipType.Legs)]
	public class Hellabyssal_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Red;
            Item.defense = 16;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.07f;
            player.moveSpeed += 0.10f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ObsidianPants, 1)
                .AddIngredient(ItemID.HellstoneBar, 20)
                .AddIngredient(ItemID.SoulofNight, 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
