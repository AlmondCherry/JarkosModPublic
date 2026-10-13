using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace jarkos.items.accs.special
{
	public class IreAttraction : ModItem
	{
		public override void SetStaticDefaults()
		{
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 7));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 70;
			Item.height = 64;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.wingTimeMax += player.wingTimeMax * 60 / 100;
			player.GetDamage(DamageClass.Generic) += 0.05f;
		}
	}
}
