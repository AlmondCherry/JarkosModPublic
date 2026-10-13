using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.armor.bastion
{
    public class BastionResolveBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
        }
    }
}
