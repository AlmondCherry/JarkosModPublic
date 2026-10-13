using jarkos.items.weapons.summon.staff.LunarDisaster;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.summonbuff
{
	public class Darkmatter_buff : ModBuff
	{
		public static readonly int TagDamage = 5;
		public static readonly int TagDamageSpecific = 15;
		public static readonly float TagDamageMultiplier = TagDamage / 100f;
		public override void SetStaticDefaults() 
		{
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}
	public class Darkmatter_buff_NPC : GlobalNPC
	{
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
				return;
			var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
			if (npc.HasBuff<Darkmatter_buff>())
			{
				modifiers.FlatBonusDamage += Darkmatter_buff.TagDamage * projTagMultiplier;
			}
			if (npc.HasBuff<Darkmatter_buff>() && projectile.type == ModContent.ProjectileType<LunarDisaster_proj>())
			{
				modifiers.FlatBonusDamage += Darkmatter_buff.TagDamageSpecific * projTagMultiplier;
			}
		}
	}
}
