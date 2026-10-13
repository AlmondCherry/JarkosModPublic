using jarkos.npc.tenderote;
using Terraria.ModLoader;
namespace jarkos.achievements
{
public class TenderoteBossKilled : ModAchievement
{
	public override void SetStaticDefaults() {
		AddNPCKilledCondition(ModContent.NPCType<Tenderote_head>());
	}
	public override Position GetDefaultPosition() => new After("EYE_ON_YOU");
}
}
