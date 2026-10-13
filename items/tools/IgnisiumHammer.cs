using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.tools
{
	public class IgnisiumHammer : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 11;
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 2;
			Item.useAnimation = 46;
			Item.scale = 1.75f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.hammer = 59;
		}
		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.NextBool(10))
			{
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.YellowTorch);
			}
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 8)
				.AddRecipeGroup(nameof(ItemID.GoldHammer), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
