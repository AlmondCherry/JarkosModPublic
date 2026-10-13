using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.items.materials;

namespace jarkos.items.accs.general
{
	[AutoloadEquip(EquipType.Wings)]
	public class VoronWings : ModItem {
		public override void SetStaticDefaults()
		{
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(1, 3f, 1f);
		}
		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.wingAccRunSpeed += 0.10f;
			player.noFallDmg = true;
		}
		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 1f;
      		ascentWhenRising = 0.175f;
      		maxCanAscendMultiplier = 1.2f;
      		maxAscentMultiplier = 1.25f;
      		constantAscend = 0.15f;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FeatherBlack>(), 26)
			.AddIngredient(ItemID.Bone, 140)
			.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 60)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
