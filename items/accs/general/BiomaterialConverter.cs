using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.accs.general
{
	public class BiomaterialConverter : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 48;
			Item.height = 48;
			Item.accessory = true;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
            var modPl = player.GetModPlayer<Jarkos_player>();
			modPl.biomaterialConverter = true;
		}
	}
}
