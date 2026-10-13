using jarkos.buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.potions
{
	public class MidasPotionItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 30;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.buffType = ModContent.BuffType<MidasPotionBuff>();
			Item.buffTime = 14400;
		}
	}
}
