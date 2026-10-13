using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Magic_powered : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = false;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
	  {
      player.GetDamage(DamageClass.Magic) += 0.20f;
      player.manaCost -= 0.20f;
	  }
  }
}
