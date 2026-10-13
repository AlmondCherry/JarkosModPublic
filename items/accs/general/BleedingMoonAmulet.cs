using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.general
{
	public class BleedingMoonAmulet : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 70;
			Item.height = 70;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			modPl.bleeding_moon_amulet = true;
			player.GetDamage(DamageClass.Generic) += 0.04f;
			if (Main.bloodMoon)
			{
				player.GetDamage(DamageClass.Generic) += 0.06f;
			}
			player.statLifeMax2 -= player.statLifeMax2 * 2 / 100;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			var jarkosplayer = player.GetModPlayer<Jarkos_player>();
			if (!jarkosplayer.Efflux_Crystal_eqipped)
			{
				return true;
			}
			return false;
		}
	}
}
