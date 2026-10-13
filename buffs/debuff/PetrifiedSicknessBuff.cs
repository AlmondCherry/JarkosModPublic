using System;
using PetrifiedAbyss.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
	public class PetrifiedSicknessBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = false;
			Main.buffNoSave[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			AbyssSicknessPlayer sickness = player.GetModPlayer<AbyssSicknessPlayer>();
			if (sickness.SicknessStack <= 0)
			{
				player.DelBuff(buffIndex);
				buffIndex--;
				return;
			}
			player.buffTime[buffIndex] = 60 * 60;
		}
		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			Player player = Main.LocalPlayer;
			if (player is null)
			{
				return;
			}
			AbyssSicknessPlayer sickness = player.GetModPlayer<AbyssSicknessPlayer>();
			if (sickness is null)
			{
				return;
			}
		}
	}
}
