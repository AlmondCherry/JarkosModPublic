using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.mounts
{
    public class Turkish_saddle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item79;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<jarkos.mounts.Rhinobeetle_mount>();
        }
    }
}
