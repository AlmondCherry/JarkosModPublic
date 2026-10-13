using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;

namespace jarkos.items.usable
{
	public class ReturnScroll : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
            Item.consumable = true;
        }
        public override bool? UseItem(Player player)
        {
            player.Spawn(PlayerSpawnContext.RecallFromItem);
            return true;
        }
	}
	public class ReturnScrollPlayer : ModPlayer
    {
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Player.ConsumeItem(ModContent.ItemType<ReturnScroll>()))
            {
                Player.statLife = 1;
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int buffType = Player.buffType[i];
                    if (buffType > 0 && Main.debuff[buffType])
                    {
                        Player.DelBuff(i);
                        i--;
                    }
                }

                Player.immune = true;
                Player.immuneTime = 180;
                Player.RemoveAllGrapplingHooks();
                Player.Spawn(PlayerSpawnContext.RecallFromItem);
                SoundEngine.PlaySound(SoundID.Item6, Player.Center);

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.MagicMirror);
                }

                Main.NewText("The Return Scroll crumbles into dust, saving your life...", 255, 215, 0);
                return false;
            }
            return true;
        }
    }
}
