using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.rare;

namespace jarkos.items.armor.anticvar
{
	[AutoloadEquip(EquipType.Legs)]
	public class Anticvar_boots : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.defense = 24;
            Item.rare = ModContent.RarityType<SpeedRarity>();
        }
        public override void UpdateEquip(Player player)
        {
           player.moveSpeed += 0.18f;
           player.accRunSpeed += 0.18f;
           player.wingTimeMax += 5;
           player.aggro -= 250;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 20)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 20)
            .AddIngredient(ModContent.ItemType<ElementalFluid>(), 160)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
