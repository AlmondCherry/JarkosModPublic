using Terraria;
using Terraria.ModLoader;
using jarkos.mounts;
namespace jarkos.buffs
{
    public class Rhinobeetle_mount_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<Rhinobeetle_mount>(), player);
            player.buffTime[buffIndex] = 10;
        }
    }
}
