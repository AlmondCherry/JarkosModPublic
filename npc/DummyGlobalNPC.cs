using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc
{
	public class DummyGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public float DamageReduction { get; set; } = 0f;
		public bool DebuffVulnerable { get; set; } = true;

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (npc.type == ModContent.NPCType<Testdummy>() || npc.type == ModContent.NPCType<Testdummy_defensed>() || npc.type == ModContent.NPCType<Testdummy_respawnable>())
			{
				if (DamageReduction > 0f)
				{
					float mult = 1f - MathHelper.Clamp(DamageReduction, 0f, 1f);
					modifiers.SourceDamage *= mult;
				}
			}
		}

		public override void PostAI(NPC npc)
		{
			if (npc.type == ModContent.NPCType<Testdummy>() || npc.type == ModContent.NPCType<Testdummy_defensed>() || npc.type == ModContent.NPCType<Testdummy_respawnable>())
			{
				if (!DebuffVulnerable)
				{
					for (int i = 0; i < npc.buffType.Length; i++)
					{
						if (npc.buffType[i] > 0 && Main.debuff[npc.buffType[i]])
						{
							npc.DelBuff(i);
							i--;
						}
					}
				}
			}
		}

		public override bool PreKill(NPC npc)
		{
			if (npc.type == ModContent.NPCType<Testdummy>() || npc.type == ModContent.NPCType<Testdummy_defensed>() || npc.type == ModContent.NPCType<Testdummy_respawnable>() || npc.type == NPCID.TargetDummy)
			{
				npc.value = 0f;
				return false;
			}
			return true;
		}
	}
}
