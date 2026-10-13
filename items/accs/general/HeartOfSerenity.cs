using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.general
{
	public class HeartOfSerenity : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 70;
			Item.height = 70;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
			player.aggro -= 100;
		}
	}
}
