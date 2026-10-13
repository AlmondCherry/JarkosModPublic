using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class Arterial_whip_tag : ModBuff
	{
		public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
    public class ArterialWhipTagNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool taggedBySoulReaper;
        public override void ResetEffects(NPC npc)
        {
            taggedBySoulReaper = false;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (taggedBySoulReaper)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                npc.lifeRegen -= 16;
                damage = 4;
                if (Main.rand.NextBool(3))
                {
                    Dust mark = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.PurpleTorch, 0f, -2f, 100, default, 1.2f);
                    mark.noGravity = true;
                }
            }
        }
        public override void ModifyHitByProjectile(NPC npc,Projectile projectile,ref NPC.HitModifiers modifiers)
        {
            if (taggedBySoulReaper && ProjectileID.Sets.MinionShot[projectile.type])
            {
                modifiers.FlatBonusDamage += 8f;
            }
        }
        public override bool PreAI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<Arterial_whip_tag>()))
            {
                taggedBySoulReaper = true;
            }
            return true;
        }
    }
}
