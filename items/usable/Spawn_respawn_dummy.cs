using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.npc;

namespace jarkos.items.usable
{
    public class Spawn_respawn_dummy : ModItem
    {
        public override string Texture => "jarkos/items/usable/Spawn_dummy";

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 1;
            Item.consumable = false;
        }

        public override bool? UseItem(Player player)
        {
            NPC.NewNPC(null, (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, ModContent.NPCType<Testdummy_respawnable>());
            return true;
        }
    }
}
