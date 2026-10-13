using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class Cirno_buff : ModBuff
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/CirnoBuff";

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Cirno_minion>()] > 0)
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
