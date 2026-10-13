using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;

namespace jarkos.items.usetiles
{
    public class ResearchTableItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ResearchTableTile>());
            Item.width = 28;
            Item.height = 22;
            Item.value = Item.sellPrice(copper: 20);
            Item.rare = ItemRarityID.LightPurple;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 20)
                .AddIngredient(ItemID.Lens, 1)
                .AddIngredient(ItemID.Book, 1)
                .AddRecipeGroup(nameof(ItemID.IronBar), 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public abstract class BaseEssenceItem : ModItem
    {
        protected abstract EssenceType Type { get; }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.maxStack = 9999;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 15;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(silver: 1);
            Item.consumable = true;
            Item.UseSound = SoundID.Item29;
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;
            
            var plr = player.GetModPlayer<ResearchPlayer>();
            plr.AddEssence(Type, 1);

            if (ModContent.GetInstance<Jarkos_config>()?.ShowEssencePickupText ?? true)
            {
                CombatText.NewText(player.getRect(), EssencePalette.ColorOf(Type), "+1 " + EssencePalette.DisplayName(Type));
            }
            return true;
        }
    }
}
