using jarkos.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace jarkos.core.PlayerInteract
{
    public class ResearchInputPlayer : ModPlayer
    {
        public override void SetControls()
        {
            if (!ResearchScreen.Instance.Active) return;
            Player.controlUp = Player.controlDown = Player.controlLeft = Player.controlRight = false;
            Player.controlJump = Player.controlUseItem = Player.controlUseTile = false;
            Player.controlThrow = Player.controlHook = Player.controlMount = false;
            Player.controlInv = Player.controlSmart = Player.controlTorch = false;
            Player.controlLeft = Player.controlRight = false;
            Player.releaseUseItem = false;
        }
        public override bool CanUseItem(Item item)
            => !ResearchScreen.Instance.Active;
        public override bool PreItemCheck()
        {
            return !ResearchScreen.Instance.Active;
        }
        public override void PostUpdate()
        {
            if (!ResearchScreen.Instance.Active) return;
            Player.itemAnimation = 0;
            Player.itemTime = 0;
            Main.mapFullscreen = false;
            Main.playerInventory = false;
        }
    }
}
