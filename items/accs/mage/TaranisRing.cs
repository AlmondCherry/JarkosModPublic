using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using jarkos.items.materials;

namespace jarkos.items.accs.mage
{
	public class TaranisRing : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 60;
			Item.height = 36;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
				modPlayer.taranisringattack = true;

				player.GetModPlayer<Jarkos_player>().TaranisAccessoryEquipped = true;
				player.GetDamage(DamageClass.Magic) += 0.15f;
				player.GetAttackSpeed(DamageClass.Magic) += 0.10f;

				player.manaMagnet = true;
				player.manaFlower = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.MagnetFlower, 1)
			.AddIngredient(ItemID.SorcererEmblem, 1)
			.AddIngredient(ModContent.ItemType<LunarCrystal>(), 15)
			.AddIngredient(ModContent.ItemType<ElementalFluid>(), 5)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
