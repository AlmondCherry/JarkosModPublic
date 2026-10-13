using jarkos.items.weapons.summon.staff.LunarDisaster;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.summonbuff
{
	public class Perfection_whip_mark : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PerfectionMarkGlobalNPC>().marked = true;
        }
    }
    public class PerfectionMarkGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool marked = false;
        public override void ResetEffects(NPC npc)
        {
            marked = false;
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (marked && ProjectileID.Sets.MinionShot[projectile.type])
            {
                modifiers.FinalDamage *= 1.08f;
            }
        }
    }
}
