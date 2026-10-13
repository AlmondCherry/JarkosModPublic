using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.accs.summon;

namespace jarkos.buffs.buff
{
  public class Nature_Gods_Bless : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = false;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetDamage(DamageClass.Summon) += 0.06f;
        player.GetAttackSpeed(DamageClass.Summon) += 0.04f;
        player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.04f;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        int maxStacks = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && !npc.friendly)
            {
                var globalNPC = npc.GetGlobalNPC<NatureSingularityGlobalNPC>();
                if (globalNPC != null && globalNPC.Stacks > maxStacks)
                {
                    maxStacks = globalNPC.Stacks;
                }
            }
        }

        if (maxStacks > 0)
        {
            tip = Language.GetTextValue("Mods.jarkos.Buffs.Nature_Gods_Bless.Description", maxStacks);
        }
    }
  }
}
