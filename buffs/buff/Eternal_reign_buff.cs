using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class Eternal_reign_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 5;
            
            int timeLeft = player.buffTime[buffIndex];
            if (timeLeft > 900) timeLeft = 900;
            
            float ratio = timeLeft / 900f;
            int hpBonus = (int)(200 * ratio);
            
            player.statLifeMax2 += hpBonus;
        }
    }
}
