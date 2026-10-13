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
	public class AntiquePickaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 186;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 36;
			Item.useTime = 2;
			Item.useAnimation = 16;
			Item.rare = ModContent.RarityType<SpeedRarity>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.pick = 250;
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
				.AddIngredient(ModContent.ItemType<AntiqueBar>(), 12)
				.AddIngredient(ModContent.ItemType<AncientFragment>(), 20)
				.AddIngredient(ModContent.ItemType<ElementalFluid>(), 60)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
