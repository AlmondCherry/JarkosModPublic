using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class PunisherConcussion : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_31";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.blind = true;
        }
    }
}
