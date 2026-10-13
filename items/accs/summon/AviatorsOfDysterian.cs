using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace jarkos.items.accs.summon
{
	public class AviatorsOfDysterian : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 6));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 20;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			modPlayer.AviatorsOfDysterian = true;
			player.wingTimeMax += player.wingTimeMax * 20 / 100;
			player.moveSpeed += 0.15f;
		}
        public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
        {
            speed += 0.10f;
			acceleration += 0.10f;
        }
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.EmpressFlightBooster, 1)
			.AddIngredient(ItemID.WingsStardust, 1)
			.AddIngredient(ItemID.WingsSolar, 1)
			.AddIngredient(ItemID.WingsNebula, 1)
			.AddIngredient(ItemID.WingsVortex, 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
