using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
    public class AstralPoison : ModBuff
    {
        private const int NpcDrain = 240;
        private const int PlayerDrain = 60;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.lifeRegen > 0)
            {
                npc.lifeRegen = 0;
            }
            npc.lifeRegen -= NpcDrain;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen = Math.Min(player.lifeRegen, 0);
            player.lifeRegen -= PlayerDrain;
        }
    }
}
