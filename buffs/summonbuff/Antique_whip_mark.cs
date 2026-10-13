using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class Antique_whip_mark : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<AntiquarianMarkGlobalNPC>().antiquarianMarked = true;
        }
    }
    public class AntiquarianMarkGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool antiquarianMarked = false;
        
        public override void ResetEffects(NPC npc)
        {
            antiquarianMarked = false;
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!antiquarianMarked) return;
            if (!ProjectileID.Sets.MinionShot[projectile.type]) return;

            Player owner = Main.player[projectile.owner];

            float dist = Vector2.Distance(owner.Center, npc.Center);
            float distBonus = MathHelper.Clamp(dist / 1000f, 0f, 0.04f);

            modifiers.FinalDamage *= 1.06f + distBonus;
        }
    }
}
