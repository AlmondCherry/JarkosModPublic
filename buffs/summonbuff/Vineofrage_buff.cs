using jarkos.items.weapons.summon.staff.PhantomDagger;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.summonbuff
{
	public class Vineofrage_buff : ModBuff
	{
		public static readonly int TagDamage = 2;
		public static readonly int TagDamageSpecific = 6;
		public static readonly float TagDamageMultiplier = TagDamage / 100f;
		public override void SetStaticDefaults()
		{
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}
	public class Vineofrage_buff_NPC : GlobalNPC
	{
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) 
		{
			if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
				return;
			var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
			if (npc.HasBuff<Vineofrage_buff>())
			{
				modifiers.FlatBonusDamage += Vineofrage_buff.TagDamage * projTagMultiplier;
			}
			if (npc.HasBuff<Vineofrage_buff>() && projectile.type == ModContent.ProjectileType<PhantomDagger_proj>()) 
			{
				modifiers.FlatBonusDamage += Vineofrage_buff.TagDamageSpecific * projTagMultiplier;
			}
		}
	}
}
