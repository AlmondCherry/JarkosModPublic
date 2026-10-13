using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.buffs.buff;

namespace jarkos.items.potions
{
	public class GradualSmallHealingPotion : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(silver: 50);
        }
        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(ModContent.BuffType<GradualSmallHealingPotion_buff>());
        }
        public override bool? UseItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<GradualSmallHealingPotion_buff>(), 600);
            return true;
        }
	}
}
