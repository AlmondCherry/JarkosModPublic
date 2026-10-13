using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.core.PlayerInteract;

namespace jarkos.items
{
    public class EssenceResetter : ModItem
    {
        public override string Texture => "Terraria/Images/Item_1508";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = 0;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
        }
        public override bool? UseItem(Player player)
        {
            var rp = player.GetModPlayer<ResearchPlayer>();
            rp.Essences.Clear();
            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText("All accumulated essences have been reset!", 255, 50, 50);
            }
            return true;
        }
    }
}
