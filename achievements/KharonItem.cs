using jarkos.items.weapons.melee.other_melee;
using jarkos.npc.tenderote;
using Terraria;
using Terraria.ModLoader;
namespace jarkos.achievements
{
public class KharonItem : ModAchievement
{
	public override void SetStaticDefaults() {
		AddItemCraftCondition(ModContent.ItemType<KharonHammer>());
	}
	public override Position GetDefaultPosition() => new After("EYE_ON_YOU");
}
}
