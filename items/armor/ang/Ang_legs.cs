using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.ang
{
	[AutoloadEquip(EquipType.Legs)]
	public class Ang_legs : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 12;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.15f;
            player.maxMinions += 1;
            player.moveSpeed += 0.05f;
        }
    }
}
