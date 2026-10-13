using jarkos.items.accs.bloodmage;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class BloodInfection : ModBuff
  {
     public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<CrimsonPulseGlobalNPC>().hasBloodInfection = true;
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood, 0f, -2f, 100, default, 1f);
                dust.noGravity = true;
            }
        }
  }
}
