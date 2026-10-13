using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class PurgatorySanctuaryBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.pvpBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.lifeRegen < 0)
                player.lifeRegen = 0;
            player.lifeRegen += 20;
            player.manaRegenBonus += 20;
            player.endurance += 0.5f;
        }
    }
}
