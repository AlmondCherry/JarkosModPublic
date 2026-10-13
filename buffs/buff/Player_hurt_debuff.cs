using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Player_hurt_debuff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= player.lifeRegen * 25 / 100;
        }
	}
}
