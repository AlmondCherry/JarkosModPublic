using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
	public class Splitting_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.lifeRegen -= 16;
			if (Main.rand.NextBool(2)) 
			{
				int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.TerraBlade);
				Main.dust[dust].scale = 0.5f;
				Main.dust[dust].noGravity = true;
			}
		}
	}
}
