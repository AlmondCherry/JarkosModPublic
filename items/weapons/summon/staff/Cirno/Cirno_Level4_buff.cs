using System;
using Terraria;
using Terraria.ModLoader;
using jarkos.items.weapons.summon.staff.FoolishCirno;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class Cirno_Level4_buff : ModBuff
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/CirnoBuff";

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			int lilCirnoCount = player.ownedProjectileCounts[ModContent.ProjectileType<LilCirno_minion>()];

			float mult = Math.Max(0f, 1.0f - lilCirnoCount * 0.10f);

			if (mult > 0f)
			{
				player.GetDamage(DamageClass.Summon) += 0.20f * mult;
				player.GetArmorPenetration(DamageClass.Generic) += (int)Math.Round(20f * mult);
				player.GetModPlayer<CirnoTagPlayer>().cirnoTagDamage = 6f * mult;
			}
		}
	}
}
