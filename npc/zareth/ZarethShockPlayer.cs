using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.zareth
{
    public class ZarethShockPlayer : ModPlayer
    {
        public bool shocked;
        public override void ResetEffects() => shocked = false;
        public override void PostUpdate()
        {
            if (shocked && Player.whoAmI == Main.myPlayer)
            {
                int idx = Player.FindBuffIndex(ModContent.BuffType<ZarethShockDebuff>());
                if (idx >= 0)
                {
                    int timeLeft = Player.buffTime[idx];
                    ZarethEffectsSystem.MaintainShock(MathHelper_Clamp(timeLeft / 240f));
                }
            }
        }
        private static float MathHelper_Clamp(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
