using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace jarkos.items.accs.mage
{
	public class GauntletOfMage : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 10));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 54;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<Jarkos_player>().magegauntlet01 = true;
			player.GetAttackSpeed(DamageClass.Magic) += 0.02f;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.FeralClaws, 1)
			.AddIngredient(ItemID.ManaRegenerationBand, 1)
			.AddIngredient(ItemID.LesserManaPotion, 250)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
