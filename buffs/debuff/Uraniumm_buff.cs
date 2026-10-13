using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace jarkos.buffs.debuff
{
	public class Uraniumm_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.lifeRegen -= 15;
			
			if (Main.rand.NextBool(2)) 
			{
				int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.TerraBlade);
				Main.dust[dust].scale = 0.5f;
				Main.dust[dust].noGravity = true;
			}
		}
	}
}
