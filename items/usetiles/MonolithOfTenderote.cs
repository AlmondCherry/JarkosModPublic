using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.items.usetiles
{
    public class MonolithOfTenderote : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.accessory = true;
            Item.vanity = true;
        }
        public override void UpdateEquip(Player player)
        {
            if (player.whoAmI == Main.myPlayer && !SkyManager.Instance["TenderoteSky"].IsActive())
            {
                SkyManager.Instance.Activate("TenderoteSky");
            }
        }
        public override void UpdateVanity(Player player)
        {
            if (player.whoAmI == Main.myPlayer && !SkyManager.Instance["TenderoteSky"].IsActive())
            {
                SkyManager.Instance.Activate("TenderoteSky");
            }
        }
    }
}
