using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class SpearDeathDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<SpearDeathGlobalNPC>().spearDeathDebuff = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            float speed = player.velocity.Length();
            float speedFactor = MathHelper.Clamp(speed / 12f, 0f, 1f);
            int regenLoss = (int)MathF.Round(MathHelper.Lerp(1f, 10f, speedFactor));
            if (player.lifeRegen > 0)
            {
                player.lifeRegen = 0;
            }
            player.lifeRegen -= regenLoss;
        }
    }
}
