using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace jarkos.items.usable
{
	public class ReforgeScroll : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }
	}
	public class ReforgeScrollSystem : GlobalItem
    {
        public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
        {
            if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.HasItem(ModContent.ItemType<ReforgeScroll>()))
            {
                reforgePrice = 0;
                return false;
            }
            return base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
        }
        public override void PostReforge(Item item)
        {
            Player player = Main.LocalPlayer;

            if (Main.netMode != NetmodeID.Server && player.HasItem(ModContent.ItemType<ReforgeScroll>()))
            {
                player.ConsumeItem(ModContent.ItemType<ReforgeScroll>());
                
                SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.5f }, player.Center);
                for (int i = 0; i < 15; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                    Dust.NewDust(player.position, player.width, player.height, DustID.Enchanted_Gold, speed.X, speed.Y);
                }
            }
        }
    }
}
