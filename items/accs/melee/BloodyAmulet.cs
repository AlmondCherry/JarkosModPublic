using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.melee
{
	public class BloodyAmulet : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<Jarkos_player>().bloodyamulet = true;
		}
	}
}
