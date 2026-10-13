using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.items.armor.bastion
{
    [AutoloadEquip(EquipType.Head)]
    public class BastionHelm : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
        public override void SetStaticDefaults()
        {
            SetBonusText = this.GetLocalization(nameof(SetBonusText));
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 22;
            Item.defense = 12;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 3);
        }
        public override void UpdateEquip(Player player)
        {
            player.statLifeMax2 += 40;
            player.GetDamage(DamageClass.Melee) += 0.08f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<BastionBreastplate>()
                && legs.type == ModContent.ItemType<BastionGreaves>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            player.GetModPlayer<BastionPlayer>().HasBastionSet = true;
        }
    }
}
