using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace jarkos.buffs.debuff
{
	public class Uranium_debuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			modPl.timerin_radioactive++;

			if(modPl.timerin_radioactive > 49)
            {
                modPl.timerin_radioactive = 0;
            }

			if(modPl.timerin_radioactive == 49)
            {
				int hz = player.statLife * 15 / 100;
				CombatText.NewText(player.Hitbox, Color.Red, hz, false, true);
                player.statLife -= hz;
            }
			if (Main.rand.NextBool(2)) 
			{
				int dust = Dust.NewDust(player.position, player.width, player.height, DustID.TerraBlade);
				Main.dust[dust].scale = 0.5f;
				Main.dust[dust].noGravity = true;
			}
		}
	}
}
