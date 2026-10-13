using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
	public class GalvanicSlowDebuff : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_32";

		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.velocity *= 0.7f;
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric);
				dust.velocity = npc.velocity * 0.2f;
				dust.scale = 0.8f;
				dust.noGravity = true;
			}
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.moveSpeed *= 0.7f;
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Electric);
				dust.velocity = player.velocity * 0.2f;
				dust.scale = 0.8f;
				dust.noGravity = true;
			}
		}
	}
}
