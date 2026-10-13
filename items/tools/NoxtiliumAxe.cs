using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.tools
{
	public class NoxtiliumAxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 21;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 30;
			Item.useTime = 12;
			Item.useAnimation = 24;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.axe = 20;
		}
		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.NextBool(10))
			{
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.PurpleTorch);
			}
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<NoxtiliumBar>(), 4)
				.AddIngredient(ItemID.Amethyst, 10)
				.AddRecipeGroup(nameof(ItemID.MythrilBar), 14)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
