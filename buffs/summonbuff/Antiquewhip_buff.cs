using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class Antiquewhip_buff : ModBuff
	{
		public static readonly int TagDamage = 40;

		public override void SetStaticDefaults() 
		{
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}
	public class Antiquewhip_buff_NPC : GlobalNPC
	{
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
				return;
				
			var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
			if (npc.HasBuff<Antiquewhip_buff>())
			{
				modifiers.FlatBonusDamage += Antiquewhip_buff.TagDamage * projTagMultiplier;
			}
		}
	}
}
