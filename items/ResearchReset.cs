using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.core;
using jarkos.core.PlayerInteract;

namespace jarkos.items
{
    public class ResearchReset : ModItem
    {
        public override string Texture => "Terraria/Images/Item_1508";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item4;
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var rp = player.GetModPlayer<ResearchPlayer>();
                ResearchSystem.GlobalCompleted.Clear();
                
                rp.AbortResearch();
                GraphState.RecomputeAll(rp);

                Main.NewText("All studies have been successfully reset!", Color.Cyan);
            }
            return true;
        }
    }
}
