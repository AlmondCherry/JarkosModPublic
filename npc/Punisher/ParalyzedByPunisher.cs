using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class ParalyzedByPunisher : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_47";
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
            player.controlUseTile = false;
            player.controlHook = false;
            player.controlMount = false;
            player.velocity.X = 0f;
            player.velocity.Y = -1.5f;
            player.maxFallSpeed = 0f;
            player.gravity = 0f;
        }
    }
}
