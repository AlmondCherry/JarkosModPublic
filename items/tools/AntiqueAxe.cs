using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.items.usetiles;
using jarkos.items.materials;
using jarkos.rare;

namespace jarkos.items.tools
{
	public class AntiqueAxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 112;
			Item.DamageType = DamageClass.Melee;
			Item.width = 34;
			Item.height = 32;
			Item.useTime = 2;
			Item.useAnimation = 24;
			Item.scale = 1.50f;
			Item.rare = ModContent.RarityType<SpeedRarity>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 4;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.axe = 25;
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
				.AddIngredient(ModContent.ItemType<AntiqueBar>(), 8)
				.AddIngredient(ModContent.ItemType<AncientFragment>(), 12)
				.AddIngredient(ModContent.ItemType<ElementalFluid>(), 40)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
