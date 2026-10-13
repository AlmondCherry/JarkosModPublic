using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Better_crate_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			var ModPlayer = player.GetModPlayer<Jarkos_fishing>();
			ModPlayer.better_potion_crate_buff = true;
		}
	}
}
