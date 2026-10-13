using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using System;

namespace jarkos.items.accs.special
{
	public class CryingFungus : ModItem
	{
		private int timer = 0;
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 48;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			timer++;
			
			if(timer > 20)
			{
				timer = 0;
			}
			if(Math.Abs(player.velocity.X) > 2f && Math.Abs(player.velocity.X) < 3f && timer == 5)
			{
				player.Heal(Main.rand.Next(1, 3));
			}
			if(Math.Abs(player.velocity.X) > 14f && timer == 5)
			{
				player.Heal(Main.rand.Next(4, 8));
			}
			player.lifeRegen = 0;
		}
	}
}
