using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.buffs.buff;

namespace jarkos.items.potions
{
	public class Fallen_star_moonshine : ModItem
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
			Item.width = 40;
			Item.height = 44;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.buffType = ModContent.BuffType<Fallen_star_moonshine_buff>();
			Item.buffTime = 12000;
		}
	}
}
