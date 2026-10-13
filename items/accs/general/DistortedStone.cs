using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.general
{
	public class DistortedStone : ModItem
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
			var modPl = player.GetModPlayer<Distorted_stone_heal>();
			modPl.Distorted_stone = true;
			player.statLifeMax2 += 150;
			float depth_of_player = player.position.Y * 0.001f;
			if (depth_of_player > 9)
			{
				depth_of_player = 9;
			}
			else if (depth_of_player < 3)
			{
				depth_of_player = 3;
			}
			player.lifeRegen += (int)depth_of_player;
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
	public class Distorted_stone_heal : ModPlayer
	{
		public bool Distorted_stone = false;
		public override void ResetEffects()
		{
			Distorted_stone = false;
		}
		public override void UpdateDead()
		{
			ResetEffects();
		}
		public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
		{
			if (quickHeal && Distorted_stone)
			{
				healValue += healValue * 50 / 100;
			}
		}
	}
}
