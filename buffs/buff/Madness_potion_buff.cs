using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.Localization;

namespace jarkos.buffs.buff
{
	public class Madness_potion_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    	{
			Player player = Main.LocalPlayer;
      		var modPl = player.GetModPlayer<Jarkos_player>();

			tip = Language.GetTextValue("Mods.jarkos.Buffs.Madness_potion_buff.Description", Math.Round(modPl.random_endurance * 100, 2), Math.Round(modPl.random_defense * 100, 2), Math.Round(modPl.random_attack_speed * 100, 2), Math.Round(modPl.random_damage * 100, 2));
			/*tip = $" {modPl.random_endurance} " + "\n"
			+ $" {modPl.random_defense} " + "\n"
			+ $" {modPl.random_attack_speed} " + "\n"
			+ $" {modPl.random_damage} " + "\n";*/
    	}
		public override void Update(Player player, ref int buffIndex)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();

			Math.Round(modPl.random_endurance, 3);
			Math.Round(modPl.random_defense, 3);
			Math.Round(modPl.random_attack_speed, 3);
			Math.Round(modPl.random_damage, 3);

			player.endurance += modPl.random_endurance;
			player.statDefense *= modPl.random_defense;

			player.GetAttackSpeed(DamageClass.Generic) += modPl.random_attack_speed;
			player.GetDamage(DamageClass.Generic) += modPl.random_damage;
		}
	}
}
