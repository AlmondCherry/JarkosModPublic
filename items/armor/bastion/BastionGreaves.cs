using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.bastion
{
    [AutoloadEquip(EquipType.Legs)]
    public class BastionGreaves : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.defense = 14;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 3, silver: 50);
        }
        public override void UpdateEquip(Player player)
        {
            player.noKnockback = true;
            player.moveSpeed += 0.08f;
        }
    }
}
