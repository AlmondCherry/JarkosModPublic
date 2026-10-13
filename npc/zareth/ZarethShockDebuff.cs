using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.zareth
{
    public class ZarethShockDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed *= 0.5f;
            player.GetDamage(DamageClass.Generic) *= 0.85f;
            player.GetModPlayer<ZarethShockPlayer>().shocked = true;
        }
    }
}
