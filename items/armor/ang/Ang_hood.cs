using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace jarkos.items.armor.ang
{
	[AutoloadEquip(EquipType.Head)]
	public class Ang_hood : ModItem
	{
	    public static LocalizedText SetBonusText { get; private set; }
	    public override void SetStaticDefaults()
        {
		    SetBonusText = this.GetLocalization(nameof(SetBonusText));
	    }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.defense = 7;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
			return body.type == ModContent.ItemType<Ang_breastplace>() && legs.type == ModContent.ItemType<Ang_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.25f;
            player.GetArmorPenetration(DamageClass.Summon) += 10;
            player.GetArmorPenetration(DamageClass.SummonMeleeSpeed) += 10;
            player.setBonus = SetBonusText.Value;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.12f;
            player.maxMinions += 1;
        }
    }
}
