using jarkos.items.weapons.summon.staff.LunarDisaster;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.summonbuff
{
	public class Divider_summontag : ModBuff
	{
		public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<InnerDissectorGlobalNPC>().taggedByDissector = true;
        }
    }
    public class InnerDissectorGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool taggedByDissector = false;
        public override void ResetEffects(NPC npc)
        {
            taggedByDissector = false;
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (taggedByDissector && ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type] > 0)
            {
                modifiers.FlatBonusDamage += 8f;
            }
        }
    }
}
