using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class AncientNatureMark : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<AncientNatureMarkGlobalNPC>().ancientMarked = true;
        }
    }
    public class AncientNatureMarkGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool ancientMarked = false;
        public override void ResetEffects(NPC npc)
        {
            ancientMarked = false;
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!ancientMarked) return;
            if (!ProjectileID.Sets.MinionShot[projectile.type]) return;
            modifiers.FinalDamage *= 1.10f;
        }
    }
}
