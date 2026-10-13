using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class PunisherKicked : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_103";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.controlLeft = false;
            player.controlRight = false;
            player.controlJump = false;
            player.controlDown = false;
            player.controlUp = false;
            player.controlUseItem = false;
            
            player.GetModPlayer<PunisherPlayer>().isKicked = true;
        }
    }
}
