using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.buffs.buff;

namespace jarkos.items.potions
{
	public class Potion01 : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
			ItemID.Sets.DrinkParticleColors[Type] = new Color[3]
			{
				new Color(240, 240, 240), new Color(200, 200, 200), new Color(140, 140, 140)
			};
		}
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.buffType = ModContent.BuffType<Amnesia_buff>();
			Item.buffTime = 12000;
		}
	}
}
