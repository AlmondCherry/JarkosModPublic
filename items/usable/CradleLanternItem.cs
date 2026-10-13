using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.cradle_biome;

namespace jarkos.items.usable
{
    public class CradleLanternItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            Lighting.AddLight(player.Center, 0.95f, 0.75f, 0.45f);

            if (player.InModBiome<PetrifiedAbyssBiome>() && player.Center.Y > Main.worldSurface * 16f)
            {
                player.lifeRegen += 7;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 10)
                .AddIngredient(ItemID.Torch, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
