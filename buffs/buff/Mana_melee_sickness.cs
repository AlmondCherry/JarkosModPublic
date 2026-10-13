using Terraria;
using Terraria.ModLoader;
namespace jarkos.buffs.buff
{
  public class Mana_melee_sickness : ModBuff
  {
    private int defense_reduce = 20;
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = false;
      Main.debuff[Type] = true;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = false;
    }
    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
      tip = $" {defense_reduce} ";
    }
    public override void Update(Player player, ref int buffIndex)
	  {
      player.statDefense -= 20;
	  }
  }
}
