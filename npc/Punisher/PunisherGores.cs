using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class PunisherWingOne : ModGore
    {
        public override void SetStaticDefaults()
        {
            GoreID.Sets.SpecialAI[Type] = 1;
        }
    }
    public class PunisherWingsTwo : ModGore
    {
        public override void SetStaticDefaults()
        {
            GoreID.Sets.SpecialAI[Type] = 1;
        }
    }
}
