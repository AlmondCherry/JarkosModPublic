using Terraria;
using Terraria.ModLoader;
namespace jarkos.buffs
{
    public class GlaciesSlowBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_46";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<npc.GlaciesGlobalNPC>().HasGlaciesSlow = true;
        }
    }
}
