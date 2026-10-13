using jarkos.buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc
{
	public class MidasGlobalNPC : GlobalNPC
	{
		public override bool PreKill(NPC npc)
		{
			if (!npc.friendly && npc.value > 0)
			{
				int killerIndex = npc.lastInteraction;
				if (killerIndex < 0 || killerIndex >= Main.maxPlayers || !Main.player[killerIndex].active || Main.player[killerIndex].dead)
				{
					killerIndex = Player.FindClosest(npc.position, npc.width, npc.height);
				}

				if (killerIndex >= 0 && killerIndex < Main.maxPlayers && Main.player[killerIndex].active && !Main.player[killerIndex].dead)
				{
					Player killer = Main.player[killerIndex];
					if (killer.HasBuff(ModContent.BuffType<MidasPotionBuff>()))
					{
						npc.value *= 2.5f;

						if (Main.netMode != NetmodeID.Server)
						{
							for (int i = 0; i < 12; i++)
							{
								Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(16, 16), DustID.GoldCoin, Main.rand.NextVector2Circular(2, 2), Scale: 1.2f);
								d.noGravity = true;
							}
						}
					}
				}
			}
			return true;
		}
	}
}
