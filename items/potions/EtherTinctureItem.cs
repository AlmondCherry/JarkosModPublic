using jarkos.buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.potions
{
	public class EtherTinctureItem : ModItem
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
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.buffType = ModContent.BuffType<EtherTinctureBuff>();
			Item.buffTime = 10800;
		}
	}
}
