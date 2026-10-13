using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.hellabyssal
{
	[AutoloadEquip(EquipType.Body)]
	public class Hellabyssal_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Red;
            Item.defense = 22;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.08f;
            player.maxMinions += 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ObsidianShirt, 1)
                .AddIngredient(ItemID.HellstoneBar, 25)
                .AddIngredient(ItemID.SoulofNight, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
