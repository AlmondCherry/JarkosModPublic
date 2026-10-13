using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.items.materials;
namespace jarkos.items.accs.mage
{
	[AutoloadEquip(EquipType.Wings)]
	public class TaranisWings : ModItem
	{
		public override void SetStaticDefaults()
		{
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}
		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			modPlayer.taranis_wings = true;
			player.wingTimeMax += 50;
			player.wingAccRunSpeed += 0.10f;
			player.noFallDmg = true;
		}
        public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
        {
			speed *= 1.10f;
			acceleration *= 1.10f;
        }
		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 1f;
      		ascentWhenRising = 0.175f;
      		maxCanAscendMultiplier = 1.2f;
      		maxAscentMultiplier = 3.25f;
      		constantAscend = 0.15f;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.LongRainbowTrailWings, 1)
			.AddIngredient(ModContent.ItemType<LunarCrystal>(), 5)
			.AddIngredient(ModContent.ItemType<ElementalFluid>(), 5)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
