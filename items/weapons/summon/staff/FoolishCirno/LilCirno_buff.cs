using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.FoolishCirno
{
	public class LilCirno_buff : ModBuff
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/FoolishCirno/LilCirnoBuff";

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LilCirno_minion>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
