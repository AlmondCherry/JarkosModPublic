using jarkos.items.accs.melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Rage_eye_buff : ModBuff
  {
    public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<VengefulEyePlayer>().aggressionActive = true;
        }
  }
}
