using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.bastion
{
    [AutoloadEquip(EquipType.Body)]
    public class BastionBreastplate : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.defense = 19;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 4);
        }
        public override void UpdateEquip(Player player)
        {
            player.endurance += 0.06f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 20)
                .AddIngredient(ItemID.HellstoneBar, 14)
                .AddIngredient(ItemID.Obsidian, 25)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
