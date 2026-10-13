using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class CirnoTagPlayer : ModPlayer
	{
		public float cirnoTagDamage = 0f;

		public override void ResetEffects()
		{
			cirnoTagDamage = 0f;
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (proj.minion || proj.DamageType == DamageClass.Summon || ProjectileID.Sets.MinionShot[proj.type])
			{
				if (cirnoTagDamage > 0f)
				{
					modifiers.FlatBonusDamage += cirnoTagDamage;
				}
			}
		}
	}
}
