using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.ang
{
	[AutoloadEquip(EquipType.Body)]
	public class Ang_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 26;
            Item.defense = 16;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.06f;
            player.maxMinions += 2;
        }
    }
}
