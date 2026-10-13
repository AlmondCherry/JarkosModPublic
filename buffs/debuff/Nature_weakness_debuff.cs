using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class Nature_weakness_debuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<NatureWeaknessGlobalNPC>().hasNatureWeakness = true;
        }
    }

    public class NatureWeaknessGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool hasNatureWeakness;

        public override void ResetEffects(NPC npc)
        {
            hasNatureWeakness = false;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (hasNatureWeakness)
            {
                modifiers.SourceDamage *= 1.2f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (hasNatureWeakness)
            {
                modifiers.SourceDamage *= 1.2f;
            }
        }
    }
}
